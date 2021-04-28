#!/bin/bash

cd tiles/ || exit
for D in $(find . -maxdepth 1 -mindepth 1 -type d -printf '%f\n')
do
#  echo "$D"
  cd "$D" || exit
  tar -czvf ../../deploy/jagfx-map-"$D".tar.gz *
  cd ..
done
