using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Utils.Enums
{
    public class AnimationsList
    {
        #region Public static fields
        public static AnimationsList[] AnimList = new AnimationsList[]
        {
            // Commun
            new AnimationsList("Commun", new Animation("Toi", "", "gestures@m@standing@casual" , "gesture_point")),
            new AnimationsList("Commun", new Animation("Viens", "", "gestures@m@standing@casual" , "gesture_come_here_soft")),
            new AnimationsList("Commun", new Animation("Quoi?", "", "gestures@m@standing@casual" , "gesture_bring_it_on")),
            new AnimationsList("Commun", new Animation("A moi", "", "gestures@m@standing@casual" , "gesture_me")),
            new AnimationsList("Commun", new Animation("Je le savais!", "", "anim@am_hold_up@male" , "shoplift_high")),
            new AnimationsList("Commun", new Animation("Etre épuisé", "", "amb@world_human_jog_standing@male@idle_b" , "idle_d")),
            new AnimationsList("Commun", new Animation("Je suis dans la merde", "", "anim@mp_player_intcelebrationmale@face_palm" , "face_palm")),
            new AnimationsList("Commun", new Animation("Facepalm", "", "anim@mp_player_intcelebrationmale@face_palm" , "face_palm")),
            new AnimationsList("Commun", new Animation("Calme-toi", "", "gestures@m@standing@casual" , "gesture_easy_now")),
            new AnimationsList("Commun", new Animation("Qu'est ce que j'ai fait ?", "", "oddjobs@assassinate@multi@" , "react_big_variations_a")),
            new AnimationsList("Commun", new Animation("Avoir peur", "", "amb@code_human_cower_stand@male@react_cowering" , "react_big_variations_a")),
            new AnimationsList("Commun", new Animation("Fight ?", "", "anim@deathmatch_intros@unarmed" , "intro_male_unarmed_e")),
            new AnimationsList("Commun", new Animation("C'est pas Possible!", "", "gestures@m@standing@casual" , "gesture_damn")),
            new AnimationsList("Commun", new Animation("Enlacer", "", "mp_ped_interaction" , "kisses_guy_a")),
            new AnimationsList("Commun", new Animation("Doigt d'honneur", "", "mp_player_int_upperfinger" , "mp_player_int_finger_01_enter")),
            new AnimationsList("Commun", new Animation("Branleur", "", "mp_player_int_upperwank" , "mp_player_int_wank_01")),

            // Provocation
            new AnimationsList("Provocation", new Animation("Célebration Vulgaire", "", "anim@mp_player_intcelebrationfemale@air_shagging" , "air_shagging")),
            new AnimationsList("Provocation", new Animation("Célebration Vulgaire 2", "", "anim@mp_player_intcelebrationfemale@dock" , "dock")),
            new AnimationsList("Provocation", new Animation("Doigt d'honneur", "", "anim@mp_player_intcelebrationfemale@finger" , "finger")),
            new AnimationsList("Provocation", new Animation("Lancer une crotte de nez", "", "anim@mp_player_intcelebrationfemale@nose_pick" , "nose_pick")),
            new AnimationsList("Provocation", new Animation("Faire craquer ses doigts", "", "anim@mp_player_intupperknuckle_crunch" , "idle_a")),
            new AnimationsList("Provocation", new Animation("Se gratter le fion", "", "friends@frt@ig_1" , "trevor_impatient_wait_3")),
            new AnimationsList("Provocation", new Animation("Attente Prostituée", "", "amb@world_human_prostitute@french@base" , "base")),
            new AnimationsList("Provocation", new Animation("Attente Prostituée 2", "", "amb@world_human_prostitute@hooker@idle_a" , "idle_a")),
            new AnimationsList("Provocation", new Animation("Faire la poule", "", "anim@mp_player_intcelebrationfemale@chicken_taunt" , "chicken_taunt")),
            new AnimationsList("Provocation", new Animation("NO WAY", "", "anim@mp_player_intcelebrationfemale@no_way" , "no_way")),
            new AnimationsList("Provocation", new Animation("Peace", "", "anim@mp_player_intcelebrationfemale@peace" , "peace")),
            new AnimationsList("Provocation", new Animation("Pied de nez", "", "anim@mp_player_intcelebrationfemale@thumb_on_ears" , "thumb_on_ears")),
            new AnimationsList("Provocation", new Animation("Faire signe que tu est fou", "", "anim@mp_player_intcelebrationmale@you_loco", "you_loco")),
            new AnimationsList("Provocation", new Animation("Chut", "", "anim@mp_player_intcelebrationfemale@shush", "shush")),

            // Danse
            new AnimationsList("Danse", new Animation("Danse ", "", "amb@world_human_partying@female@partying_beer@base" , "base")),
            new AnimationsList("Danse", new Animation("Danse Sexy", "", "mini@strip_club@idles@stripper" , "stripper_idle_04")),
            new AnimationsList("Danse", new Animation("Danse Sexy 2", "", "mini@strip_club@lap_dance@ld_girl_a_song_a_p1" , "ld_girl_a_song_a_p1_f")),
            new AnimationsList("Danse", new Animation("Danse Sexy il parait", "", "mini@strip_club@idles@stripper" , "stripper_idle_05")),
            new AnimationsList("Danse", new Animation("Danse de beauf", "", "special_ped@mountain_dancer@monologue_3@monologue_3a" , "mnt_dnc_buttwag")),
        };
        #endregion

        #region Public Fields
        public string CategorieName;
        public Animation Animation;
        #endregion

        #region Constructor
        public AnimationsList(string categorieName, Animation animation)
        {
            CategorieName = categorieName;
            Animation = animation;
        }
        #endregion
    }
}