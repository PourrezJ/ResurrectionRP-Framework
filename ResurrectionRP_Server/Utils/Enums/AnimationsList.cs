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

            // Militaire
            new AnimationsList("Militaire", new Animation("Salut militaire", "","mp_player_int_uppersalute",  "mp_player_int_salute_enter")),
            new AnimationsList("Militaire", new Animation("Okay!", "","swat",  "understood")),
            new AnimationsList("Militaire", new Animation("Stop!", "","mini@strip_club@idles@bouncer@stop",  "stop")),
            new AnimationsList("Militaire", new Animation("Attente mains croisées", "","amb@world_human_stand_guard@male@base",  "base")),
            new AnimationsList("Militaire", new Animation("Mains sur la ceinture", "","amb@world_human_cop_idles@male@base",  "base")),
            new AnimationsList("Militaire", new Animation("Faire signe de circuler", "","amb@world_human_car_park_attendant@male@base",  "base")),
            new AnimationsList("Militaire", new Animation("Ecrire sur le carnet", "","amb@medic@standing@timeofdeath@base",  "base")),
            new AnimationsList("Militaire", new Animation("Examen du corps", "","amb@medic@standing@tendtodead@idle_a",  "idle_a")),
            new AnimationsList("Militaire", new Animation("Accroupi, examen du corps", "","amb@medic@standing@tendtodead@base",  "base")),
            new AnimationsList("Militaire", new Animation("Mains sur les hanches", "","amb@code_human_police_investigate@base",  "base")),

            //  Celebrate
            new AnimationsList("Célébration", new Animation("Sauter de joie !", "","amb@world_human_cheering@female_a",  "base")),
            new AnimationsList("Célébration", new Animation("Applaudir", "","amb@world_human_cheering@female_d",  "base")),
            new AnimationsList("Célébration", new Animation("Applaudir doucement", "","amb@world_human_cheering@male_e",  "base")),
            new AnimationsList("Célébration", new Animation("Envoie de bisous", "","anim@mp_player_intcelebrationfemale@blow_kiss",  "blow_kiss")),
            new AnimationsList("Célébration", new Animation("Celebration avec les mains", "","anim@mp_player_intcelebrationmale@jazz_hands",  "jazz_hands")),
            new AnimationsList("Célébration", new Animation("Secouer les mains en l'air", "","anim@mp_player_intcelebrationfemale@surrender",  "surrender")),
            new AnimationsList("Célébration", new Animation("Pouce en l'air", "","anim@mp_player_intcelebrationfemale@thumbs_up",  "thumbs_up_facial")),
            new AnimationsList("Célébration", new Animation("La reine d'angleterre", "","anim@mp_player_intcelebrationfemale@wave",  "wave_facial")),
            new AnimationsList("Célébration", new Animation("Saluer le public, et applaudir", "","anim@mp_player_intcelebrationpaired@m_m_sarcastic",  "sarcastic_right_facial")),
            new AnimationsList("Célébration", new Animation("Victoire!", "","mini@dartsintro_alt1",  "darts_ig_intro_alt1_guy2_face")),
            new AnimationsList("Célébration", new Animation("La statu", "","amb@world_human_statue@base",  "base")),
            new AnimationsList("Célébration", new Animation("L'italien", "","anim@mp_player_intcelebrationmale@finger",  "finder")),
            // Assis / Allongé
            new AnimationsList("Célébration", new Animation("Méditation", "","rcmcollect_paperleadinout@",  "meditiate_idle")),
            new AnimationsList("Célébration", new Animation("Allonger sur le ventre", "","amb@world_human_sunbathe@female@front@base",  "base")),
            new AnimationsList("Célébration", new Animation("Allonger sur le dos", "","amb@world_human_sunbathe@female@back@base",  "base")),
            new AnimationsList("Célébration", new Animation("Assis par terre homme", "","amb@world_human_picnic@male@base",  "base")),
            new AnimationsList("Célébration", new Animation("Assis par terre femme", "","amb@world_human_picnic@female@base",  "base")),
            new AnimationsList("Célébration", new Animation("Allonger sur le côté", "","amb@world_human_bum_slumped@male@laying_on_left_side@base",  "base")),
            new AnimationsList("Célébration", new Animation("Adossé sur le mur", "","amb@world_human_leaning@male@wall@back@foot_up@idle_a",  "idle_a")),
            new AnimationsList("Célébration", new Animation("Faire du yoga", "","amb@world_human_yoga@female@base",  "base_b")),
            new AnimationsList("Célébration", new Animation("Assis stupeur", "","amb@world_human_stupor@male@base",  "base")),

            //Danse
            new AnimationsList("Danse", new Animation("Air guitare", "","amb@world_human_musician@guitar@male@base",  "base")),
            new AnimationsList("Danse", new Animation("Air guitare rock", "","anim@mp_player_intcelebrationfemale@air_guitar",  "air_guitar")),
            new AnimationsList("Danse", new Animation("Air piano", "","anim@mp_player_intcelebrationfemale@air_synth",  "air_synth")),
            new AnimationsList("Danse", new Animation("Air DJ", "","anim@mp_player_intcelebrationfemale@dj",  "dj_facial")),

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
            new AnimationsList("Provocation", new Animation("Lancer une crotte de nez", "","anim@mp_player_intcelebrationfemale@nose_pick",  "nose_pick")),
            new AnimationsList("Provocation", new Animation("Faire craquer les doigts", "","anim@mp_player_intupperknuckle_crunch",  "idle_a")),
            new AnimationsList("Provocation", new Animation("Se gratter les fesses", "","friends@frt@ig_1",  "trevor_impatient_wait_2")),
            new AnimationsList("Provocation", new Animation("CHEH", "","anim@mp_player_intcelebrationfemale@shush",  "shush")),
            new AnimationsList("Provocation", new Animation("Dépité", "","anim@mp_player_intcelebrationfemale@face_palm",  "face_palm")),
            new AnimationsList("Provocation", new Animation("Pied de nez", "","anim@mp_player_intcelebrationfemale@thumbs_up",  "thumbs_up_facial")),

            // SPORT
            new AnimationsList("Sport", new Animation("Courir sur place Femme", "","amb@world_human_jog_standing@female@base",  "base")),
            new AnimationsList("Sport", new Animation("Courir sur place Homme", "","amb@world_human_jog_standing@male@fitbase",  "base")),
            new AnimationsList("Sport", new Animation("Montrer ses muscles", "","amb@world_human_muscle_flex@arms_at_side@base",  "base")),
            new AnimationsList("Sport", new Animation("Montrer ses muscles", "","amb@world_human_muscle_flex@arms_in_front@idle_a",  "idle_b")),
            new AnimationsList("Sport", new Animation("Faire ses pompes", "","amb@world_human_push_ups@male@base",  "base")),
            new AnimationsList("Sport", new Animation("Faire des abdos", "","amb@world_human_sit_ups@male@base",  "base")),
            new AnimationsList("Sport", new Animation("Femme Marcher sur places", "","amb@world_human_power_walker@female@static",  "static")),
            new AnimationsList("Sport", new Animation("Pousser de la fonte", "","amb@prop_human_seat_muscle_bench_press@idle_a",  "idle_a")),
            new AnimationsList("Sport", new Animation("Faire des tractions", "","amb@prop_human_muscle_chin_ups@male@base",  "base")),

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