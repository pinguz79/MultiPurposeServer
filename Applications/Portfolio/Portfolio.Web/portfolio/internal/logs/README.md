# Portfolio.Web logs

PHP errors and application diagnostics are written explicitly to `portfolio.log` by `AppLogger`, without relying solely on the hosting configuration for `log_errors` and `error_log`.

The log file is generated at runtime, excluded from version control and protected from HTTP access by `.htaccess`. It can be inspected through the Altervista file manager or FTP.

The deployed PHP process must have write permission on this directory.

On the first request after deployment, the logger creates the file and writes a `Logging initialized` entry. If the file is not created, the deployed PHP process does not have write permission on the directory.
