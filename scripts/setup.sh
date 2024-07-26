#!/bin/sh

script_path=$(dirname "$(realpath "$0")")
project_root=$(dirname "$(dirname "$(realpath "$0")")")
cp  $script_path/pre-commit.sh $project_root/.git/hooks/pre-commit
chmod +x $project_root/.git/hooks/pre-commit
