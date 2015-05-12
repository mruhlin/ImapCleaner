using ImapX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImapX.Enums;
using System.Text.RegularExpressions;

namespace ImapCleaner {
    /// <summary>
    /// For a given IMAP mailbox, iterates and deletes all empty folders.
    /// </summary>
    class ImapCleaner {
        private static bool _testMode = false;

        // This is kind of dumb.  We don't want to delete leaf-level folders in pluck.  i.e. if a user's Inbox is full but his Trash is empty, and we delete the Trash folder, pluck will NOT auto-create the Trash folder next time it needs to use it.
        private static Regex _isComponentFolder = new Regex(@"INBOX\.[0-9]+\.[0-9]+\.[^\s]+\.(Inbox|Ignored|Sent|Trash)"); 

        private static List<string> _foldersToDelete = new List<string>(); 
        

        static void Main(string[] args) {
            var server = args[0];
            var user = args[1];
            var password = args[2];

            if (args.Length > 3) {
                _testMode = (args[3].Equals("-t"));
                Console.Out.WriteLine("Test Mode.  Won't actually delete anything.");
            }

            var client = new ImapClient(server); // TODO : SSL blah blah, only meeting one use case right now...
            client.Behavior.MessageFetchMode = MessageFetchMode.Flags;
            //client.Behavior.FolderTreeBrowseMode = FolderTreeBrowseMode.Full;
            client.Behavior.ExamineFolders = true;

            if (!client.Connect()) {
                Console.Error.WriteLine("Failed to connect!");
                Environment.Exit(1);
            }

            if (!client.Login(user, password)) {
                Console.Error.WriteLine("Login failed.  Check username/password");
                Environment.Exit(1);
            }
            foreach (var folder in client.Folders) {
                CleanFolder(folder);
            }

            if (!_testMode) {
                ExecuteDeletions(client);
            }
        }

        private static void ExecuteDeletions(ImapClient client) {
            foreach (var deleteFolder in _foldersToDelete) {
                Console.Out.WriteLine("Execute deletion: " + deleteFolder);
                var folderPath = deleteFolder.Split('.');
                var folderToDelete = client.Folders[folderPath[0]];
                for (int i = 1; i < folderPath.Length; i++) {
                    folderToDelete = folderToDelete.SubFolders[folderPath[i]];
                }

                // delete any subfolders.
                foreach (var subfolder in folderToDelete.SubFolders.ToArray()) {
                    if(subfolder.Selectable){
                        subfolder.Remove();
                    }
                }

                if(folderToDelete.Selectable){
                    folderToDelete.Remove();
                }
            }
        }

        private static bool CleanFolder(Folder folder) {
            // check if any messages.  Do nothing if there are.
            if (folder.Selectable && folder.Exists > 0) {
                Console.Out.WriteLine("Folder " + folder.Path + " has messages.  Do not delete");
                return false;
            }

            // check in subfolders
            bool subFoldersEmpty = true;
            foreach (var subFolder in folder.SubFolders) {
                if (!CleanFolder(subFolder)) {
                    subFoldersEmpty = false;
                }
                else {
                    DeleteFolder(subFolder);
                }
            }
            return subFoldersEmpty;
        }

        private static void DeleteFolder(Folder folder) {
            if (_isComponentFolder.IsMatch(folder.Path)) {
                return;
            }

            Console.Out.WriteLine("Delete folder: " + folder.Path);
            _foldersToDelete.Add(folder.Path);
        }
    }

}
