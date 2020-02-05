#!/usr/bin/env bash

set -x

echo "Testing for $TEST_PLATFORM"

docker pull microsoft/dotnet:latest
dotnet --info

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -runTests \
  -testPlatform $TEST_PLATFORM \
  -testResults $(pwd)/$TEST_PLATFORM-results.xml \
  -logFile \
  -batchmode

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 134 ]; then

  ${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
    -projectPath $(pwd) \
    -runTests \
    -testPlatform $TEST_PLATFORM \
    -testResults $(pwd)/$TEST_PLATFORM-results.xml \
    -logFile \
    -batchmode

  UNITY_EXIT_CODE=$?

fi

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

cat $(pwd)/$TEST_PLATFORM-results.xml | grep test-run | grep Passed


git clone "https://gitlab.com/alexjbinnie/extentreports-dotnetcore-cli.git"
cd extentreports-dotnetcore-cli/ExtentReportsDotNetCLI
dotnet publish -c Release
cd ../../
dotnet extentreports-dotnetcore-cli/ExtentReportsDotNetCLI/ExtentReportsDotNetCLI/bin/Release/netcoreapp2.1/ExtentReportsDotNetCLI.dll -i $(pwd)/$TEST_PLATFORM-results.xml
mv index.html $TEST_PLATFORM-results.html

exit $UNITY_EXIT_CODE
