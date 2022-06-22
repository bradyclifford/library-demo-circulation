REM Install the Certificate authority cert into the Trusted Root Certification Authorities store for the current user.
REM This must be run from an elevated command window

REM for other platforms: https://www.bounca.org/tutorials/install_root_certificate.html

certutil -addstore Root %~dp0ca.crt

pause