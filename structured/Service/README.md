
https://learn.microsoft.com/pt-br/dotnet/iot/deployment

"System.Globalization.Invariant": true

## Removed Support for FTP

The reader no longer supports FTP. It is replaced by SFTP. The service can be accessed using the login ‘sftp.’ The default password is 'impinj', but should be changed during reader setup to a unique, secure value. The SFTP password can be changed via the RShell command ‘config network sftp server password [old_password] [new_password]’.

Note: There is a known bug in version 8.0.1.240 where the SFTP user only has read/write permissions in the /tmp and /scratch directories. To enable SFTP support for the /customer directory, execute the following command from within the reader's OSShell.

chmod a+rwx /customer