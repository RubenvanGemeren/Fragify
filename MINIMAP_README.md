# Interactive Map Feature for Fragify

This feature adds interactive maps from [Total CS](https://totalcsgo.com) to the Fragify web dashboard, displaying detailed map callouts and information when playing CS:GO/CS2.

## Features

- **Interactive map display**: Shows detailed interactive maps with callouts for the current map being played
- **Total CS integration**: Embeds maps from Total CS, the premier CS:GO/CS2 callout resource
- **Automatic map switching**: Maps automatically update when the game map changes
- **Responsive design**: Interactive maps scale appropriately on different screen sizes
- **External links**: Direct links to open maps in new tabs for full-screen viewing

## How It Works

1. **Map Detection**: The system automatically detects which map is currently being played
2. **Interactive Map Loading**: Loads the corresponding interactive map from Total CS
3. **Embedded Display**: Shows the interactive map in an iframe within the Game Information card
4. **Callout Information**: Users can hover over map locations to see detailed callout information

## Available Interactive Maps

The system includes interactive maps for popular CS:GO/CS2 maps from Total CS:

- **de_dust2** - Dust II
- **de_mirage** - Mirage
- **de_inferno** - Inferno
- **de_cache** - Cache
- **de_overpass** - Overpass
- **de_nuke** - Nuke
- **de_ancient** - Ancient
- **de_vertigo** - Vertigo
- **de_train** - Train
- **de_cobblestone** - Cobblestone
- **de_canals** - Canals
- **de_biome** - Biome
- **de_breach** - Breach
- **de_climb** - Climb
- **de_engage** - Engage
- **de_guard** - Guard
- **de_mutiny** - Mutiny
- **de_ruby** - Ruby
- **de_sirocco** - Sirocco
- **de_studio** - Studio
- **de_swamp** - Swamp
- **de_tulip** - Tulip
- **de_anubis** - Anubis
- **de_aztec** - Aztec
- **de_dust** - Dust
- **de_prodigy** - Prodigy
- **de_survivor** - Survivor
- **de_tuscan** - Tuscan

## Map Source

The system embeds interactive maps from:

1. **[Total CS](https://totalcsgo.com)**: The premier CS:GO/CS2 callout resource with detailed interactive maps
2. **Professional Quality**: High-quality maps with accurate callout information
3. **Regular Updates**: Maps are maintained and updated by the Total CS team

## Configuration

### Adding Custom Map URLs

You can add custom interactive map URLs by editing the `getInteractiveMapUrl` function in the JavaScript code:

```javascript
function getInteractiveMapUrl(mapName) {
    const mapUrlMap = {
        'de_dust2': 'https://totalcsgo.com/callouts/dust2',
        'de_custom': 'https://your-custom-map-url.com'
    };

    return mapUrlMap[mapName.toLowerCase()] || null;
}
```

### Programmatic Addition

You can also add URLs programmatically by modifying the map URL mapping object.

## Troubleshooting

### No Interactive Map Displayed

1. Check if the map name is being detected correctly
2. Verify that the Total CS map URLs are accessible
3. Check browser console for error messages
4. Ensure the web interface is running on the correct port

### Map Loading Issues

1. The system embeds maps directly from Total CS
2. If maps fail to load, a fallback message is displayed with external link
3. Check network connectivity and firewall settings
4. Verify that Total CS is accessible from your network
5. Some networks may block iframe content - use the external link as alternative

## Technical Details

### Files Modified

- `UI/WebInterface.cs` - Added interactive map display and iframe embedding
- `Services/MinimapImageService.cs` - No longer needed (replaced with direct Total CS integration)
- `Services/WebMapThemeService.cs` - Updated to support interactive maps

### API Endpoints

- `GET /api/theme?mapName={mapName}` - Returns theme with map information
- The interactive map is automatically updated when the map changes

### JavaScript Functions

- `updateMinimap(mapName)` - Updates the interactive map display
- `getInteractiveMapUrl(mapName)` - Returns the Total CS URL for a given map

## Future Enhancements

- **Local map caching**: Store interactive maps locally for offline access
- **Custom map overlays**: Add player positions, bomb sites, etc.
- **Map-specific themes**: Enhanced visual themes based on map aesthetics
- **Enhanced interactivity**: Additional map features and callout information
- **Map statistics**: Track map performance and win rates

## Contributing

To add support for new maps:

1. Add the map name and Total CS URL to the `getInteractiveMapUrl` function
2. Test with the web interface
3. Submit a pull request with your changes

## Support

If you encounter issues with the interactive map feature:

1. Check the console logs for error messages
2. Verify that the map names match exactly (case-sensitive)
3. Test the Total CS URLs directly in a browser
4. Use the external link if iframe embedding fails
5. Report issues with detailed error information
