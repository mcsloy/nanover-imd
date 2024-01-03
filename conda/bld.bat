REM @echo on

mkdir %SCRIPTS%
robocopy %RECIPE_DIR%\artifacts\StandaloneWindows64 %SCRIPTS%\NarupaImd /e
REM Make NarupaImd available in the Path while keeping it in
REM its directory.
set local_script=%%CONDA_PREFIX%%\Scripts%
echo "%local_script%\NarupaImd\Narupa iMD.exe" > %SCRIPTS%\NarupaImd.bat
exit 0
