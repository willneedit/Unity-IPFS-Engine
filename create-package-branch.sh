#!/bin/bash

PKG_ROOT=Assets/Sample-Project

if [ "x$1" != "x" ]; then
  git tag raw-$1
fi

git branch -D package &> /dev/null || echo "package branch not found (but it's okay)"
git checkout -b package
git clean -fdx
git rm -rf "*"
git restore --staged "${PKG_ROOT}"
git restore "${PKG_ROOT}"
git mv ${PKG_ROOT}/* .
rm -rf "${PKG_ROOT}"
git commit -am "Extracted package tree"

if [[ -d "Samples" ]]; then
  git mv Samples Samples~
  rm -f Samples.meta
  git commit -am "fix: Samples => Samples~"
fi
if [[ -d "Tests" ]]; then
  git mv Tests Tests~
  rm -f Tests.meta
  git commit -am "fix: Tests => Tests~"
fi

if [ "x$1" != "x" ]; then
  git tag $1
fi
