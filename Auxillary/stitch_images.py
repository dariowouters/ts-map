from os import system, listdir, chdir, environ


chdir("G:\\Screenshots")

upperX = 160_000 #160_000
lowerX = -120_000 #-120_000

upperY = 130_000 #130_000
lowerY = -210_000 #-210_000

scale = 4000

i = 0
for y in range(lowerY, upperY, scale * 2):
    images = []
    for x in range(lowerX, upperX, scale * 2):
        images.append(f"{x};{y}.png")

    print(images)

    system("magick " + " ".join(images) + " +append row" + str(i) + ".png")
    i += 1


rows = []
for key in listdir("."):
    if "row" in key:
        rows.append(key)

environ["MAGICK_TMPDIR"] = "G:\\Temp"
system("magick " + " ".join(rows) + " -append result.png")

print("DONE!")
