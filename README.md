# SSRSUpdateReports
C# console application that allows developers to update the XML all of their SSRS reports at once. Handy for mass front end changes.

Blog Post: http://dotnetdevelopernetwork.com/ssrs-reports-mass-update-reports/

# ReportServer
Database Items : 
dbo.ReportServer
Table: Catalog

SET UP
# 1: 
Set up your connection string. The location where the report server database is. You will need to set up to
different connection strings. One as for grabbing the report items from the server and the other for writing
those report items back to the table once the changes have been made.
Ln: 22
Ln: 115

# 2:
You can manually change the directory where you would live the xml reports to be stored. By default i have 
created and used a folder called MyTmpXMLReports on the C Drive (C:\\MyTmpXMLReports\\). Make sure this folder
exists in that location (or create another) before you run the application.

# 3: 
Make sure your access to the report server database has full read and write access. You will be reading 
from and updating items in the report server database. 

# 4: 
This console application only uses system assembilies so there is no need to install or restore any third
party packages/references.

