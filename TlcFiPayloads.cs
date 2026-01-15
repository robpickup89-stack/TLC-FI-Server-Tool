namespace TLCFiServerTool;

internal static class TlcFiPayloads
{
    internal const string TlcCertKey = "TLC_CERT";
    internal const string TlcfaKey = "TLCFA";
    internal const string TlcfaUsersKey = "TLCFA_USERS";

    internal static readonly (string Key, string Payload)[] Responses =
    {
        (TlcfaUsersKey, TlcfaUsersPayload),
        (TlcCertKey, TlcCertPayload),
        (TlcfaKey, TlcfaPayload)
    };

    internal const string TlcCertPayload = @"NT(""TLC_CERTIFICATIONS.SYS"",2){
D(""2, TLCFI,    4977606E,  SWPT TLC-FI v2.0.0,      1.2.000,  2024-03-01,  SWARCO Peek Traffic"");
D(""2, IVERATLC, 26EF6447,  SWPT IVERA-TLC v4.2.0,   1.2.000,  2024-03-01,  SWARCO Peek Traffic"");
}
";

    internal const string TlcfaPayload = @"NT(""TLCFA_LOG.CNF"",0){
}
CALL(""TLCFA_LOG.INI"",""N=tlcfa_log.log,L=500"");
P(""TLCFA_LOG_DEFAULT.R1""){
D(50);
}
NT(""TLCFA_SOCKET.CNF"",1){
D(""TLCFI"");
}
NT(""TLCFA_ID.SYS"",1){
D(""SWA_PT_0106"");
}
NT(""TLCFA_COMPANY.SYS"",1){
D(""SWARCO"");
}
NT(""TLCFA_VERSION.SYS"",1){
D(""1_1_0"");
}
NP(""TLCFA_DISABLE_READY2HAND.R1"",1,0,1,1,0){
D(0);
}
NP(""TLCFA_DISABLE_TLC_OVERRULE.R1"",1,0,1,1,0){
D(0);
}
NP(""TLCFA_DISABLE_SG_LAMPFAULT.R1"",1,0,1,1,0){
D(0);
}
NT(""TLCFA_XP.CNF"",1){
D(""STREAM1"");
}
CALL(""TLCFA_XP.INI"",""TEMPLATE=TR2500"");
P(""TLCFA_XP_TIMING_PARAM.R2""){
P(4);
D(100);
}
P(""TLCFA_XP_MAC_RESTART.R1""){
D(1);
}
P(""TLCFA_XP_WORKING_ON1.R11"",""TLCFA_XP_WORKING_OFF1.R11""){
D(0,2400);
D(0,2400);
D(0,2400);
D(0,2400);
D(0,2400);
D(0,2400);
D(0,2400);
}
NT(""TLCFA_SG_TYPE.I"",2){
D(""ig_veh"");
D(""ig_ped"");
}
CALL(""TLCFA_SG_TYPE.INI"",""TEMPLATE=INTERGREEN"");
NT(""TLCFA_SG_XP0.CNF"",9){
D(""     A,     ig_veh,   PERMISSIVE,      RED,     1,    -1,      2,     2,    7,    -1,      0,     0,     3,     3"");
D(""     B,     ig_veh,   PERMISSIVE,      RED,     1,    -1,      2,     2,    7,    -1,      0,     0,     3,     3"");
D(""     C,     ig_veh,   PERMISSIVE,      RED,     1,    -1,      2,     2,    7,    -1,      0,     0,     3,     3"");
D(""     D,     ig_ped,   PERMISSIVE,      RED,     1,    -1,      0,     0,    4,    -1,      0,    -1,     0,     0"");
D(""     E,     ig_ped,   PERMISSIVE,      RED,     1,    -1,      0,     0,    4,    -1,      0,    -1,     0,     0"");
D(""     F,     ig_veh,   PERMISSIVE,      RED,     1,    -1,      2,     2,    7,    -1,      0,     0,     3,     3"");
D(""     G,     ig_ped,   PERMISSIVE,      RED,     1,    -1,      0,     0,    4,    -1,      0,    -1,     0,     0"");
D(""     H,     ig_ped,   PERMISSIVE,      RED,     1,    -1,      0,     0,    4,    -1,      0,    -1,     0,     0"");
D(""     I,     ig_veh,   PERMISSIVE,      RED,     1,    -1,      0,     0,    1,    -1,      0,     0,     0,    -1"");
}
CALL(""TLCFA_SG.INI"","");
P(""TLCFA_SG_XP0_CONFLICT_TYPE.R1""){
D(2);
}
P(""TLCFA_SG_XP0_CONFLICT.R2""){
D(-0.1);
D(-0.1);
D(5.0);
D(5.0);
D(-0.1);
D(-0.1);
D(5.0);
D(-0.1);
D(3.0);
D(-0.1);
D(-0.1);
D(5.0);
D(5.0);
D(-0.1);
D(-0.1);
D(5.0);
D(-0.1);
D(3.0);
D(5.0);
D(5.0);
D(-0.1);
D(5.0);
D(5.0);
D(5.0);
D(5.0);
D(5.0);
D(3.0);
D(5.0);
D(5.0);
D(5.0);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(0.0);
D(-0.1);
D(-0.1);
D(5.0);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(3.0);
D(-0.1);
D(-0.1);
D(5.0);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(3.0);
D(5.0);
D(5.0);
D(5.0);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(3.0);
D(-0.1);
D(-0.1);
D(5.0);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(-0.1);
D(3.0);
D(2.0);
D(2.0);
D(2.0);
D(2.0);
D(2.0);
D(2.0);
D(2.0);
D(2.0);
D(-0.1);
}
CALL(""TLCFA_SG_SWON.INI"",""TYPE=UK,START_DELAY=10"");
P(""TLCFA_SWON_UK_INTERGREEN_MIN.R1"",""TLCFA_SWON_UK_INTERGREEN_MAX.R1"",""TLCFA_SWON_UK_INTERGREEN.R1""){
D(0,40,11);
}
P(""TLCFA_SWON_UK_XP0_PEDESTRIAN.R1""){
D(0);
D(0);
D(0);
D(1);
D(1);
D(0);
D(1);
D(1);
D(0);
}
P(""TLCFA_SWON_UK_XP0_START_STAGE.R1""){
D(0);
D(0);
D(1);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
}
CALL(""TLCFA_DET.INI"","");
T(""TLCFA_DET_XVQ.CNF""){
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
D("");
}
NT(""TLCFA_IN.CNF"",22){
D(""UTC_I1"");
D(""UTC_I2"");
D(""UTC_I3"");
D(""UTC_I4"");
D(""UTC_I5"");
D(""UTC_I6"");
D(""UTC_I7"");
D(""UTC_I8"");
D(""UTC_I9"");
D(""UTC_I10"");
D(""LEVEL3,LEVEL3/0"");
D(""CALIBRATION,XLMCAL.STS/0"");
D(""MP_MC,      XUKMP_TLC_MC.R1/0"");
D(""MP_MCSTAGE, XUKMP_TLC_MCSTAGE.R1/0"");
D(""MP_AR,      XUKMP_TLC_MCAR.R1/0"");
D(""MP_VA,      XUKMP_TLC_VA_REQ.R1/0"");
D(""MP_FT,      XUKMP_TLC_FT_REQ.R1/0"");
D(""MP_CL,      XUKMP_TLC_CL_REQ.R1/0"");
D(""MP_FKEY,    XUKMP_TLC_FKEY.STS/0"");
D(""MP_FT1,      XUKMP_TLC_FT_REQ_XP.R1/0"");
D(""MP_VA1,      XUKMP_TLC_VA_REQ_XP.R1/0"");
D(""MP_CL1,      XUKMP_TLC_CL_REQ_XP.R1/0"");
}
CALL(""TLCFA_IN.INI"","");
NT(""TLCFA_OUT.CNF"",35){
D(""Interlock_D_P5"");
D(""Interlock_D_P6"");
D(""Interlock_E_P7"");
D(""Interlock_E_P9"");
D(""Interlock_G_P11"");
D(""Interlock_G_P14"");
D(""Interlock_H_P2"");
D(""Interlock_H_P3"");
D(""Interlock_J_P15"");
D(""Interlock_J_P17"");
D(""FH1_133L"");
D(""Cycle_Wait_J"");
D(""UTC_O1"");
D(""UTC_O2"");
D(""UTC_O3"");
D(""UTC_O4"");
D(""UTC_O5"");
D(""UTC_O6"");
D(""UTC_O7"");
D(""UTC_O8"");
D(""UTC_O9"");
D(""UTC_O10"");
D(""UTC_O12"");
D(""UTC_O13"");
D(""WL_D,XSGWL.CSC/D"");
D(""WL_E,XSGWL.CSC/E"");
D(""WL_G,XSGWL.CSC/G"");
D(""WL_H,XSGWL.CSC/H"");
D(""STREAM1_MODE,  XUKMP_APP_MODE.R1/0"");
D(""STREAM1_AR,    XUKMP_APP_AR.R1/0"");
D(""STREAM1_STAGE, XUKMP_APP_MCSTAGE.R1/0"");
D(""STREAM1_IT,    XUKMP_APP_IT.R1/0"");
D(""STREAM1_MG,    XUKMP_APP_MG.R1/0"");
D(""STREAM1_SDE,   XUKMP_APP_DE.R1/0"");
D(""MP_DFM,        XUKMP_APP_DFM.R1/0"");
}
CALL(""TLCFA_OUT.INI"","");
P(""TLCFA_OUT_XP.R1""){
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(-1);
}
NT(""TLCFA_VAR.CNF"",32){
D(""UTC_I1"");
D(""UTC_I2"");
D(""UTC_I3"");
D(""UTC_I4"");
D(""UTC_I5"");
D(""UTC_I6"");
D(""UTC_I7"");
D(""UTC_I8"");
D(""UTC_I9"");
D(""UTC_I10"");
D(""UTC_I11"");
D(""UTC_I12"");
D(""UTC_I13"");
D(""UTC_I14"");
D(""UTC_I15"");
D(""UTC_I16"");
D(""UTC_O1"");
D(""UTC_O2"");
D(""UTC_O3"");
D(""UTC_O4"");
D(""UTC_O5"");
D(""UTC_O6"");
D(""UTC_O7"");
D(""UTC_O8"");
D(""UTC_O9"");
D(""UTC_O10"");
D(""UTC_O11"");
D(""UTC_O12"");
D(""UTC_O13"");
D(""UTC_O14"");
D(""UTC_O15"");
D(""UTC_O16"");
}
CALL(""TLCFA_VAR.INI"","");
P(""TLCFA_VAR_DEFAULT.R1""){
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
D(0);
}
NT(""TLCFA_VEH.CNF"",1){
D(""SpecialVehicleEvents"");
}
CALL(""TLCFA_VEH.INI"","");
NT(""TLCFA_APP.CNF"",2){
D(""SRM2UK_XP0,  CONTROL,   EXTERN,  Control-stream-1,     0,    1,        EMBEDDED"");
D(""Chameleon,   PROVIDER,  EXTERN,  Chameleon Control"");
}
CALL(""TLCFA_APP.INI"","");
P(""TLCFA_APP_XP0_STANDBY_MODE.CNF""){
D(1);
}
P(""TLCFA_XP0_TT_D.R30"",""TLCFA_XP0_TT_S.R11"",""TLCFA_XP0_TT_E.R11"",""TLCFA_XP0_TT_P.R1""){
D(127,0,2400,1);
}
NT(""XPARPIPE.CNF"",32){
D(""IN=TLCFA_VAR_VALUE.R1/0,OUT=VIRTUAL_IN/0"");
D(""IN=TLCFA_VAR_VALUE.R1/1,OUT=VIRTUAL_IN/1"");
D(""IN=TLCFA_VAR_VALUE.R1/2,OUT=VIRTUAL_IN/2"");
D(""IN=TLCFA_VAR_VALUE.R1/3,OUT=VIRTUAL_IN/3"");
D(""IN=TLCFA_VAR_VALUE.R1/4,OUT=VIRTUAL_IN/4"");
D(""IN=TLCFA_VAR_VALUE.R1/5,OUT=VIRTUAL_IN/5"");
D(""IN=TLCFA_VAR_VALUE.R1/6,OUT=VIRTUAL_IN/6"");
D(""IN=TLCFA_VAR_VALUE.R1/7,OUT=VIRTUAL_IN/7"");
D(""IN=TLCFA_VAR_VALUE.R1/8,OUT=VIRTUAL_IN/8"");
D(""IN=TLCFA_VAR_VALUE.R1/9,OUT=VIRTUAL_IN/9"");
D(""IN=TLCFA_VAR_VALUE.R1/10,OUT=VIRTUAL_IN/10"");
D(""IN=TLCFA_VAR_VALUE.R1/11,OUT=VIRTUAL_IN/11"");
D(""IN=TLCFA_VAR_VALUE.R1/12,OUT=VIRTUAL_IN/12"");
D(""IN=TLCFA_VAR_VALUE.R1/13,OUT=VIRTUAL_IN/13"");
D(""IN=TLCFA_VAR_VALUE.R1/14,OUT=VIRTUAL_IN/14"");
D(""IN=TLCFA_VAR_VALUE.R1/15,OUT=VIRTUAL_IN/15"");
D(""IN=VIRTUAL_OUT/0,OUT=TLCFA_VAR_DEFAULT.R1/16"");
D(""IN=VIRTUAL_OUT/1,OUT=TLCFA_VAR_DEFAULT.R1/17"");
D(""IN=VIRTUAL_OUT/2,OUT=TLCFA_VAR_DEFAULT.R1/18"");
D(""IN=VIRTUAL_OUT/3,OUT=TLCFA_VAR_DEFAULT.R1/19"");
D(""IN=VIRTUAL_OUT/4,OUT=TLCFA_VAR_DEFAULT.R1/20"");
D(""IN=VIRTUAL_OUT/5,OUT=TLCFA_VAR_DEFAULT.R1/21"");
D(""IN=VIRTUAL_OUT/6,OUT=TLCFA_VAR_DEFAULT.R1/22"");
D(""IN=VIRTUAL_OUT/7,OUT=TLCFA_VAR_DEFAULT.R1/23"");
D(""IN=VIRTUAL_OUT/8,OUT=TLCFA_VAR_DEFAULT.R1/24"");
D(""IN=VIRTUAL_OUT/9,OUT=TLCFA_VAR_DEFAULT.R1/25"");
D(""IN=VIRTUAL_OUT/10,OUT=TLCFA_VAR_DEFAULT.R1/26"");
D(""IN=VIRTUAL_OUT/11,OUT=TLCFA_VAR_DEFAULT.R1/27"");
D(""IN=VIRTUAL_OUT/12,OUT=TLCFA_VAR_DEFAULT.R1/28"");
D(""IN=VIRTUAL_OUT/13,OUT=TLCFA_VAR_DEFAULT.R1/29"");
D(""IN=VIRTUAL_OUT/14,OUT=TLCFA_VAR_DEFAULT.R1/30"");
D(""IN=VIRTUAL_OUT/15,OUT=TLCFA_VAR_DEFAULT.R1/31"");
}
CALL(""XPARPIPE.INI"","");
";

    internal const string TlcfaUsersPayload = @"/*
 * Copyright (c) 1998-2023 SWARCO PEEK TRAFFIC B.V.
 *
 *  File         : TLCFA_USERS.TXT
 *
 *  Project      : 106L - The Headrow / Calverley Street / East Parade Junction
 *
 */

[TLCFI]
""Chameleon"" ""CHAM2"" ""provider""

[WEB]
";
}
