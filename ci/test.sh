#!/usr/bin/env bash

set -x

apt-get update -y
apt-get install -y xsltproc

echo "Testing for $TEST_PLATFORM"

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

xsltproc -o $(pwd)/$TEST_PLATFORM-junit-results.xml ./ci/nunit3-junit.xslt $(pwd)/$TEST_PLATFORM-results.xml

exit $UNITY_EXIT_CODE
