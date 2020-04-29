#!/usr/bin/env bash

# Exit immediately if a command returns a non-zero error code
set -e

# Print trace of simple commands
set -x

echo "Building for $BUILD_TARGET"

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
