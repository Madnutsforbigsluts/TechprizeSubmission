using System.Collections.Generic;

using Glyph.Entities;

namespace Glyph
{
    public class LocationNotifier
    {

        private string _location;

        public string Location
        {

            get { return _location; }
            set
            {
                if (_location != value)
                {
                    _location = value;

                    // Remove all NPCS from the previous location
                    CreateEntityFactory.NPCS.RemoveRange(0,
                        CreateEntityFactory.NPCS.Count);
                    new CreateEntityFactory(value, "NPC"); 
                    UpdateScreenState();
                }
            }
        }

        public LocationNotifier(string location)
        {
            _location = location;
        }

        private void UpdateScreenState()
        {
            ScreenState.screenState = ScreenUpdateState.LocationUpdate; 
        }
    }
}
