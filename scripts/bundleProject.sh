#!/bin/bash

cd TsMap.Canvas/bin/Release || exit
tar -czf ../../../deploy/jagfx-ts-map.tar.gz  -c *.dll -c *.exe
