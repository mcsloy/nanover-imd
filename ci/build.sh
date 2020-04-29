#!/usr/bin/env bash

# Exit immediately if a command returns a non-zero error code
set -e

# Print trace of simple commands
set -x

echo "Building for $BUILD_TARGET"

# Generate csproj files
${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -quit \
  -batchmode \
  -executeMethod NarupaIMD.Editor.GenerateCsProj.Generate \
  -logFile /dev/stdout

ls

# Check initial mono version
mono --version

# Update Mono
apt install gnupg ca-certificates
apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list
apt update

apt install -y mono-devel

# Check mono version
mono --version

# Get the 2.5.1 version of docfx extracted to the docfx/ folder
mkdir docfx
cd docfx
wget https://github.com/dotnet/docfx/releases/download/v2.51/docfx.zip
apt-get -y install unzip
unzip docfx.zip
cd ../

mono docfx/docfx.exe metadata || mono docfx/docfx.exe metadata

# Create the folder to store the build
export BUILD_PATH=./Builds/$BUILD_TARGET/
mkdir -p $BUILD_PATH

# Run unity to build the executable.
${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -quit \
  -batchmode \
  -buildTarget $BUILD_TARGET \
  -buildWindows64Player "${BUILD_PATH}${BUILD_NAME}.exe" \
  -logFile /dev/stdout

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

ls -la $(pwd)/$BUILD_PATH
ls -la $BUILD_PATH
[ -n "$(ls -A $BUILD_PATH)" ] # fail job if build folder is empty
