# ImapCleaner

This is a simple app that solved a quick problem at work.  We have a custom messaging solution that uses an IMAP server as its backend, in kind of a non-standard way.
Each instance of the application has its own mailbox, then actual users' mailboxes exist as folders underneath the main account's mailbox. i.e. CustomerName/INBOX/hash1/hash2/userX/Inbox contains userX's Inbox, CustomerName/INBOX/hash1/hash2/userX/Trash contains userX's trash
The first time a user logs in, several folders for him are created, whether he actually uses the messaging feature or not.
So when it comes time to migrate from one server to another, the copy process spends a lot of time iterating through empty folders and checking whether they actually have any messages.  They usually don't.
So, this tool can be run in advance to quickly find and delete empty mailboxes, which speeds up the coming copy process, reducing our maintenance window.

I planned on making this a generally-applicable tool that would delete all empty folders in a mailbox, but quirks of the Pluck application demanded a custom solution.  i.e. if the user's Inbox is empty but is Sent Items folder isn't, we can't delete either.