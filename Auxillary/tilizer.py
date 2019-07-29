import math
from os import system, listdir, chdir, environ, makedirs, path
from shutil import copyfile


upperX = 160_000 #160_000
lowerX = -120_000 #-120_000

upperY = 130_000 #130_000
lowerY = -210_000 #-210_000

scale = 4000


def create_pairs(z):
    ys = listdir(f"tiles/{z + 1}")
    xs = listdir(f"tiles/{z + 1}/0")
    ys = len(ys)
    xs = len(xs)

    groups = []
    for i in range(0, ys, 2):
        for j in range(0, xs, 2):
            groups.append([
                [(i, j), (i, j + 1)],
                [(i + 1, j), (i + 1, j + 1)],
            ])

    leny = math.ceil(ys / 2)
    lenx = math.ceil(xs / 2)

    locs = []
    for y in range(leny):
        for x in range(lenx):
            locs.append((y, x))
    return groups, locs


def get_file(z, y, x):
    f = f"tiles/{z + 1}/{y}/{x}.png"
    if path.exists(f):
        return f
    else:
        return "empty.png"


for z in range(5, -1, -1):
    if z == 5:
        yi = 0
        for y in range(lowerY, upperY, scale * 2):
            makedirs(f"tiles/{z}/{yi}", exist_ok=True)
            xi = 0
            for x in range(lowerX, upperX, scale * 2):
                copyfile(f"F:/Screenshots/{x};{y}.png", f"tiles/{z}/{yi}/{xi}.png")
                print(f"{z};{yi};{xi}")
                xi += 1
            yi += 1
    else:
        pairs, locs = create_pairs(z)
        for p in pairs:
            pair = p
            l = locs[0]
            locs = locs[1:]
            makedirs(f"tiles/{z}/{l[0]}", exist_ok=True)
            cmd = f"magick ( {get_file(z, pair[0][0][0], pair[0][0][1])} {get_file(z, pair[0][1][0], pair[0][1][1])} +append ) ( {get_file(z, pair[1][0][0], pair[1][0][1])} {get_file(z, pair[1][1][0], pair[1][1][1])} +append ) -append -resize 4000x4000 tiles/{z}/{l[0]}/{l[1]}.png"
            system(cmd)
            print(cmd)