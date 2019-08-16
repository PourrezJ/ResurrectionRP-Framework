﻿const fathers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 42, 43, 44];
const mothers = [21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 45];
const fatherNames = ["Benjamin", "Daniel", "Joshua", "Noah", "Andrew", "Juan", "Alex", "Isaac", "Evan", "Ethan", "Vincent", "Angel", "Diego", "Adrian", "Gabriel", "Michael", "Santiago", "Kevin", "Louis", "Samuel", "Anthony", "Claude", "Niko", "John"];
const motherNames = ["Hannah", "Audrey", "Jasmine", "Giselle", "Amelia", "Isabella", "Zoe", "Ava", "Camila", "Violet", "Sophia", "Evelyn", "Nicole", "Ashley", "Grace", "Brianna", "Natalie", "Olivia", "Elizabeth", "Charlotte", "Emma", "Misty"];

const eyeColors = ["Vert", "Émeraude", "Bleu clair", "Bleu océan", "Marron clair", "Marron", "Noisette", "Gris", "Gris clair"];

const hairColors = [];

const hairList = [
    // male
    [
        { ID: 0, Name: "Rasage de prés", Collection: "mpbeach_overlays", Overlay: "FM_Hair_Fuzz" },
        { ID: 1, Name: "Boule à zéro", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_001" },
        { ID: 2, Name: "Crête avec côtés rasés ", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_002" },
        { ID: 3, Name: "Hipster", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_003" },
        { ID: 4, Name: "Raie d'un côté", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_004" },
        { ID: 5, Name: "Coupe courte", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_005" },
        { ID: 6, Name: "Biker", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_006" },
        { ID: 7, Name: "Queue de cheval", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_007" },
        { ID: 8, Name: "Tresse africaine", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_008" },
        { ID: 9, Name: "Lissé", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_009" },
        { ID: 10, Name: "Court peigné", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_013" },
        { ID: 11, Name: "Punk", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_002" },
        { ID: 12, Name: "Caesar", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_011" },
        { ID: 13, Name: "Coupé", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_012" },
        { ID: 14, Name: "Dreads", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_014" },
        { ID: 15, Name: "Cheveux longs", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_015" },
        { ID: 16, Name: "Frisé", Collection: "multiplayer_overlays", Overlay: "NGBea_M_Hair_000" },
        { ID: 17, Name: "Coupe de surfer", Collection: "multiplayer_overlays", Overlay: "NGBea_M_Hair_001" },
        { ID: 18, Name: "Court sur le côté", Collection: "multiplayer_overlays", Overlay: "NGBus_M_Hair_000" },
        { ID: 19, Name: "Haut et luisant", Collection: "multiplayer_overlays", Overlay: "NGBus_M_Hair_001" },
        { ID: 20, Name: "Long Slicked", Collection: "multiplayer_overlays", Overlay: "NGHip_M_Hair_000" },
        { ID: 21, Name: "Jeune Hipster", Collection: "multiplayer_overlays", Overlay: "NGHip_M_Hair_001" },
        { ID: 22, Name: "Mullet", Collection: "multiplayer_overlays", Overlay: "NGInd_M_Hair_000" },
        { ID: 24, Name: "Tresse classique", Collection: "mplowrider_overlays", Overlay: "LR_M_Hair_000" },
        { ID: 25, Name: "Tresse palmé", Collection: "mplowrider_overlays", Overlay: "LR_M_Hair_001" },
        { ID: 26, Name: "Tresse éclairée", Collection: "mplowrider_overlays", Overlay: "LR_M_Hair_002" },
        { ID: 27, Name: "Tresse spéciale", Collection: "mplowrider_overlays", Overlay: "LR_M_Hair_003" },
        { ID: 28, Name: "Tresse en zig zag", Collection: "mplowrider2_overlays", Overlay: "LR_M_Hair_004" },
        { ID: 29, Name: "Tresse en escargot", Collection: "mplowrider2_overlays", Overlay: "LR_M_Hair_005" },
        { ID: 30, Name: "80's Will Smith", Collection: "mplowrider2_overlays", Overlay: "LR_M_Hair_006" },
        { ID: 31, Name: "Coupe playmobil", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_000_M" },
        { ID: 32, Name: "Coupe balayé", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_001_M" },
        { ID: 33, Name: "Coupe balayé sur le côté", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_002_M" },
        { ID: 34, Name: "Punk", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_003_M" },
        { ID: 35, Name: "Mod", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_004_M" },
        { ID: 36, Name: "Layered Mod", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_005_M" },
        { ID: 72, Name: "Coupe plateau", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 73, Name: "Boule à zéro millitaire", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_001_M" },
        /*
        { ID: 75, Name: "Hairmod1", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 76, Name: "Hairmod2", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 77, Name: "Hairmod3", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 78, Name: "Hairmod4", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 79, Name: "Hairmod5", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 80, Name: "Hairmod6", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 81, Name: "Hairmod7", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 82, Name: "Hairmod8", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 83, Name: "Hairmod9", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 84, Name: "Hairmod10", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        //{ ID: 85, Name: "Hairmod10", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" },
        { ID: 86, Name: "Hairmod11", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_M_000_M" }*/
    ],
    // female
    [
        { ID: 0, Name: "Rasage de prés", Collection: "mpbeach_overlays", Overlay: "FM_Hair_Fuzz" },
        { ID: 1, Name: "Court", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_001" },
        { ID: 2, Name: "Layered Mod", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_002" },
        { ID: 3, Name: "Queue de cochon", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_003" },
        { ID: 4, Name: "Queue de cheval", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_004" },
        { ID: 5, Name: "Coupe tresse", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_005" },
        { ID: 6, Name: "Tresses", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_006" },
        { ID: 7, Name: "Bob", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_007" },
        { ID: 8, Name: "Faux Hawk", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_008" },
        { ID: 9, Name: "Twist Francais", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_009" },
        { ID: 10, Name: "Long Bob", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_010" },
        { ID: 11, Name: "En vrac", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_011" },
        { ID: 12, Name: "Pixie", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_012" },
        { ID: 13, Name: "Frange coupée", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_013" },
        { ID: 14, Name: "Noeud Haut", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_014" },
        { ID: 15, Name: "Bob wavy", Collection: "multiplayer_overlays", Overlay: "NG_M_Hair_015" },
        { ID: 16, Name: "Bun désordonné", Collection: "multiplayer_overlays", Overlay: "NGBea_F_Hair_000" },
        { ID: 17, Name: "Pin Up Girl", Collection: "multiplayer_overlays", Overlay: "NGBea_F_Hair_001" },
        { ID: 18, Name: "Bun serré", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_007" },
        { ID: 19, Name: "Bob tordu", Collection: "multiplayer_overlays", Overlay: "NGBus_F_Hair_000" },
        { ID: 20, Name: "Bob clapet", Collection: "multiplayer_overlays", Overlay: "NGBus_F_Hair_001" },
        { ID: 21, Name: "Big Bangs", Collection: "multiplayer_overlays", Overlay: "NGBea_F_Hair_001" },
        { ID: 22, Name: "Tresse avec un haut noeud", Collection: "multiplayer_overlays", Overlay: "NGHip_F_Hair_000"},
        { ID: 23, Name: "Mullet", Collection: "multiplayer_overlays", Overlay: "NGInd_F_Hair_000" },
        { ID: 25, Name: "Tresse pincé", Collection: "mplowrider_overlays", Overlay: "LR_F_Hair_000" },
        { ID: 26, Name: "Tresse en feuille", Collection: "mplowrider_overlays", Overlay: "LR_F_Hair_001" },
        { ID: 27, Name: "Tresse en zig zag", Collection: "mplowrider_overlays", Overlay: "LR_F_Hair_002" },
        { ID: 28, Name: "Coupe petite fille", Collection: "mplowrider2_overlays", Overlay: "LR_F_Hair_003" },
        { ID: 29, Name: "Tresse vagues", Collection: "mplowrider2_overlays", Overlay: "LR_F_Hair_003" },
        { ID: 30, Name: "Tresse chaines", Collection: "mplowrider2_overlays", Overlay: "LR_F_Hair_004" },
        { ID: 31, Name: "Coupe vintage", Collection: "mplowrider2_overlays", Overlay: "LR_F_Hair_006" },
        { ID: 32, Name: "Coupe playmobil", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_000_F" },
        { ID: 33, Name: "Coupe balayé", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_001_F" },
        { ID: 34, Name: "Coupe balayé sur le côté", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_002_F" },
        { ID: 35, Name: "Punk", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_003_F" },
        { ID: 36, Name: "Bandana et tresse", Collection: "multiplayer_overlays", Overlay: "NG_F_Hair_003" },
        { ID: 37, Name: "Layered Mod", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_006_F" },
        { ID: 38, Name: "Skinbyrd", Collection: "mpbiker_overlays", Overlay: "MP_Biker_Hair_004_F" },
        { ID: 76, Name: "Bun soigné", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_000_F" },
        { ID: 77, Name: "Bob court", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        /*{ ID: 79, Name: "Hairmod1", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 80, Name: "Hairmod2", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 81, Name: "Hairmod3", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 82, Name: "Hairmod4", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 83, Name: "Hairmod5", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 84, Name: "Hairmod6", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 85, Name: "Hairmod7", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 86, Name: "Hairmod8", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        //{ ID: 87, Name: "Hairmod9", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        //{ ID: 88, Name: "Hairmod10", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 89, Name: "Hairmod11", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 90, Name: "Hairmod12", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 91, Name: "Hairmod13", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" },
        { ID: 92, Name: "Hairmod14", Collection: "mpgunrunning_overlays", Overlay: "MP_Gunrunning_Hair_F_001_F" }*/
    ]
];