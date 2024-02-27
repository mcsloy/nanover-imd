REM @echo on

mkdir %SCRIPTS%
robocopy %RECIPE_DIR%\artifacts\StandaloneWindows64 %SCRIPTS%\NanoverImd /e
REM Make NanoverImd available in the Path while keeping it in
REM its directory.
set local_script=%%CONDA_PREFIX%%\Scripts%
echo "%local_script%\NanoverImd\Nanover iMD.exe" > %SCRIPTS%\NanoverImd.bat
exit 0
