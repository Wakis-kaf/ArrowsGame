-- created with TexturePacker (http://www.texturepacker.com)
           frames = {
             {% for sprite in sprites %}
               { x={{sprite.frameRect.x}}, y={{sprite.frameRect.y}}, width={{sprite.frameRect.width}}, height={{sprite.frameRect.height}} }, -- {{sprite.trimmedName}}{% endfor %}
           },
    
           sheetContentWidth = {{texture.size.width}},
           sheetContentHeight = {{texture.size.height}}
