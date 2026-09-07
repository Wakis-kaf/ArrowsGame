--
-- created with TexturePacker (http://www.texturepacker.com)
--
-- {{smartUpdateKey}}
--
-- local sheetInfo = require("myExportedImageSheet") -- lua file that Texture packer published
--
-- local myImageSheet = graphics.newImageSheet( "ImageSheet.png", sheetInfo:getSheet() ) -- ImageSheet.png is the image Texture packer published
--
-- local myImage1 = display.newImage( myImageSheet , sheetInfo:getFrameIndex("image_name1"))
-- local myImage2 = display.newImage( myImageSheet , sheetInfo:getFrameIndex("image_name2"))
--

local SheetInfo = {}

SheetInfo.sheet =
{
    frames = {
    {% for sprite in allSprites %}
        {
            -- {{sprite.trimmedName}}
            x={{sprite.frameRect.x}},
            y={{sprite.frameRect.y}},
            width={{sprite.frameRect.width}},
            height={{sprite.frameRect.height}},
{% if sprite.trimmed %}
            sourceX = {{sprite.sourceRect.x}},
            sourceY = {{sprite.sourceRect.y}},
            sourceWidth = {{sprite.untrimmedSize.width}},
            sourceHeight = {{sprite.untrimmedSize.height}}{% endif %}
        },{% endfor %}
    },
    
    sheetContentWidth = {{texture.size.width}},
    sheetContentHeight = {{texture.size.height}}
}

SheetInfo.frameIndex =
{
{% for sprite in allSprites %}
    ["{{sprite.trimmedName}}"] = {{ forloop.counter }},{% endfor %}
}

function SheetInfo:getSheet()
    return self.sheet;
end

function SheetInfo:getFrameIndex(name)
    return self.frameIndex[name];
end

return SheetInfo
