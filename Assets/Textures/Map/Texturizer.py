from PIL import Image
import sys

def CreateMap(sourceMaps, outputPaths):
    source_images = [Image.open(x, mode="r").convert("RGB") for x in sourceMaps]

    source_width, source_height = source_images[0].size
    new_images = [Image.new("RGBA", (source_width, source_height)) for _ in outputPaths]

    for y in range(source_height):
        print(f"Row: {y}/{source_height}")
        for x in range(source_width):
            pixels = [z.getpixel((x, y)) for z in source_images]
            new_images[0].putpixel((x, y), (pixels[0][0], pixels[0][0], pixels[0][0], 1))
            new_images[1].putpixel((x, y), (int(1-pixels[0][1]), int(1-pixels[0][1]), int(1-pixels[0][1]), 1))
            new_images[2].putpixel((x, y), (pixels[0][2], pixels[0][0], pixels[1][0], int(1-pixels[0][1])))

    [new_images[x].save(outputPaths[x]) for x in range(len(outputPaths))]

def MultiplyImage(old, new, multiplier):
    s = Image.open(old).convert("RGB")
    w, h = s.size
    o = Image.new("RGBA", (w, h))
    for y in range(h):
        for x in range(w):
            o.putpixel((x, y), tuple([max(z*multiplier, 1) for z in s.getpixel((x,y))]))
    o.save(new)

def NegateImage(old, new):
    s = Image.open(old).convert("RGB")
    w, h = s.size
    o = Image.new("RGBA", (w, h))
    for y in range(h):
        for x in range(w):
            o.putpixel((x, y), tuple([max(1-z, 1) for z in s.getpixel((x,y))]))
    o.save(new)

def Invert(old, new):
    s = Image.open(old).convert("RGBA")
    w, h = s.size
    o = Image.new("RGBA", (w, h))
    for y in range(h):
        for x in range(w):
            newPix = [0, 255, 0, int(s.getpixel((x,y))[3]-(0.2*255))]
            o.putpixel((x, y), tuple(newPix))
    o.save(new)

def FadeOut(old, new):
    s = Image.open(old).convert("RGBA")
    w, h = s.size
    o = Image.new("RGBA", (w, h))
    for y in range(h):
        for x in range(w):
            p = list(s.getpixel((x,y)))
            p[3] = p[0]
            o.putpixel((x, y), tuple(p))
    o.save(new)


# source_path = [f"{sys.argv[1]}\\MaskMap.png", f"{sys.argv[1]}\\Height.png"]
# target_path = [f"{sys.argv[1]}\\AO.png", f"{sys.argv[1]}\\Smoothness.png", f"{sys.argv[1]}\\DetailMask.png"]
# CreateMap(source_path, target_path)

# NegateImage(f"{sys.argv[1]}\\Roughness.png", f"{sys.argv[1]}\\Smoothness.png")

Invert(sys.argv[1], sys.argv[2])