#!/usr/bin/env bash

# Exit immediately if a command returns a non-zero error code
set -e

# Print trace of simple commands
set -x

# Create folders needed for
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/

# Don't print trace of simple commands
set +x

echo 'Writing $UNITY_LICENSE_CONTENT to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf'

# Copy environment variable UNITY_LICENSE_CONTENT to a license file, which needs to be obtained by following the steps laid out in https://gitlab.com/gableroux/unity3d-gitlab-ci-example. The tr command removes any carriage returns (\r) that may have appeared in the file.
echo "$UNITY_LICENSE_CONTENT" | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf

# Print trace of simple commands
set -x
