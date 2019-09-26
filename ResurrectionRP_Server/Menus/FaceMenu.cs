using AltV.Net.Async;
using AltV.Net.Async.Events;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.XMenuManager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Menus
{
    public class Mood
    {
        public string Name;
        public string Anim;
        public XMenuItemIconDesc Logo;

        public Mood(string name, string anim, XMenuItemIconDesc logo)
        {
            Name = name;
            Anim = anim;
            Logo = logo;
        }
    }

    public class FaceMenu
    {
        List<Mood> PlayerMoods = new List<Mood>
        {
            new Mood("Aiming", "mood_aiming_1", XMenuItemIcons.MEH_SOLID),
            new Mood("En colère", "mood_angry_1", XMenuItemIcons.ANGRY_SOLID),
            new Mood("Ivre", "mood_drunk_1", XMenuItemIcons.GRIN_SQUINT_TEARS_SOLID),
            new Mood("Content","mood_happy_1", XMenuItemIcons.SMILE_BEAM_SOLID),
            new Mood("Blessé","mood_injured_1", XMenuItemIcons.TIRED_SOLID),
            new Mood("Stressé", "mood_stressed_1", XMenuItemIcons.FLUSHED_SOLID),
            new Mood("Bouder", "mood_sulk_1", XMenuItemIcons.FROWN_SOLID),
        };

        private XMenu facialMenu;

        public FaceMenu()
        {
        }

        public static async Task OpenFaceMenu(IPlayer client)
        {
            FaceMenu faceMenu = new FaceMenu();

            faceMenu.facialMenu = new XMenu("ID_Face");
            faceMenu.facialMenu.CallbackAsync = faceMenu.FacialCallback;

            faceMenu.facialMenu.Add(new XMenuItem("Normal", "", "ID_Normal", XMenuItemIcons.MEH_BLANK_SOLID));

            foreach (Mood mood in faceMenu.PlayerMoods)
            {
                var item = new XMenuItem(mood.Name, "", "", mood.Logo);
                item.SetData("Mood", mood);
                faceMenu.facialMenu.Add(item);
            }

            await faceMenu.facialMenu.OpenXMenu(client);
        }

        private async Task FacialCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            /*
            if (menuItem.Id == "ID_Normal")
                await client.GetPlayerHandler().ResetFacialAnim();
            else
            {
                Mood mood = menuItem.GetData("Mood");

                if (mood != null)
                    client.GetPlayerHandler()?.SetFacialAnim(mood.Anim);        
            }
            */
            await XMenuManager.XMenuManager.CloseMenu(client);
        }
    }
}
