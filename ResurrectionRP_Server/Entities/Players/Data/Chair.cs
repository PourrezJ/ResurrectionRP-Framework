using System.Collections.Generic;
using System.Linq;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class Chair
    {
        #region Fields
        internal string task;
        internal double x;
        internal double y;
        internal double z;
        internal double h;
        #endregion

        #region Data

        public static Dictionary<string, Chair> Chairs = new Dictionary<string, Chair>()
        {
            { "prop_bench_01a", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0, h = 360}},
            { "prop_bench_01b", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_01c", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_04", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_05", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_06", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_08", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_09", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_10", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_bench_11", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_fib_3b_bench", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_ld_bench01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_wait_bench_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "hei_prop_heist_off_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "hei_prop_hei_skid_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_01a", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_01b", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_04a", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_04b", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_05", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_06", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_08", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_09", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chair_10", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_chateau_chair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_clown_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_cs_office_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_direct_chair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_direct_chair_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_gc_chair02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_off_chair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_off_chair_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_off_chair_04", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_off_chair_04b", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_off_chair_05", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_old_deck_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_old_wood_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_rock_chair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_skid_chair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_skid_chair_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_skid_chair_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_sol_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_wheelchair_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_wheelchair_01_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_armchair_01_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_clb_officechair_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_dinechair_01_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_ilev_p_easychair_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_soloffchair_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_yacht_chair_01_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_club_officechair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_corp_bk_chair3", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_corp_cd_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_chair02_ped", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_hd_chair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_p_easychair", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ret_gc_chair03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_ld_farm_chair01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_04_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_05_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_06_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_leath_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_01_chr_a", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_01_chr_b", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_02_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_03b_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_table_03_chr", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_torture_ch_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_fh_dineeamesa", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_fh_kitchenstool", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_tort_stool", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "hei_prop_yah_seat_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "hei_prop_yah_seat_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "hei_prop_yah_seat_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_yacht_seat_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_waiting_seat_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_yacht_seat_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_yacht_seat_03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_hobo_seat_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},


            { "prop_rub_couch01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "miss_rub_couch_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_ld_farm_couch01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_ld_farm_couch02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_rub_couch02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_rub_couch03", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_rub_couch04", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},

            { "p_lev_sofa_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_res_sofa_l_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_v_med_p_sofa_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "p_yacht_sofa_01_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_ilev_m_sofa", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_res_tre_sofa_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_tre_sofa_mess_a_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_tre_sofa_mess_b_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "v_tre_sofa_mess_c_s", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_roller_car_01", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}},
            { "prop_roller_car_02", new Chair(){ task = "PROP_HUMAN_SEAT_BENCH", x = 0.0, y = 0.0, z = 0.5, h = 180.0}}
        };
        #endregion

        #region Methods
        public static bool IsChair(uint hash) => Chairs.Keys.Any(c => AltV.Net.Alt.Hash(c) == hash);

        public static Chair GetChairData(uint hash)
        {
            foreach(var chairdic in Chairs)
            {
                if (IsChair(hash))
                {
                    if (GameMode.Instance.IsDebug)
                        AltV.Net.Alt.Server.LogInfo(chairdic.Key);
                    return chairdic.Value;
                }
            }
            return null;
        }
        #endregion
    }
}
