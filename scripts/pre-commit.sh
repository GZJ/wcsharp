#!/bin/sh

csFiles=$(find . -maxdepth 1 -type f -name '*.cs')
for f in $csFiles; do
    echo "Compiling $(basename $f)..."
    dotnet csharpier "$f"
done
