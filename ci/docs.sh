#!/usr/bin/env bash

# Exit immediately if a command returns a non-zero error code
set -e

# Print trace of simple commands
set -x

echo "Generating Documentation"

# Generate csproj files
${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -quit \
  -batchmode \
  -executeMethod GenerateCsProj.Generate \
  -logFile /dev/stdout

# Update Mono
apt install gnupg ca-certificates
apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list
apt update

# Install the latest version of mono develop.
apt install -y mono-devel

# Get the 2.51 version of docfx extracted to the docfx/ folder
mkdir docfx
cd docfx
wget https://github.com/dotnet/docfx/releases/download/v2.51/docfx.zip
apt-get -y install unzip
unzip docfx.zip
cd ../

mono docfx/docfx.exe metadata || mono docfx/docfx.exe metadata

mkdir docs/csproj_files

cp *.csproj docs/csproj_files
