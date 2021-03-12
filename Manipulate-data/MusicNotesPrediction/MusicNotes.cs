using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ML.MusicNotesPrediction
{
    // Chorale	Key	Measure	Note	60	61	62	63	64	65	66	67	68	69	70	71	72	73	74	75	76	77	78	79

    class MusicNotes
    {
        [Column(ordinal:"0")]
        public float Chorale;
        [Column(ordinal: "1")]
        public float Key;
        [Column(ordinal: "2")]
        public float Measure;
        [Column(ordinal: "3")]
        public string Note;
        [Column(ordinal: "4")]
        public float N_60;
        [Column(ordinal: "5")]
        public float N_61;
        [Column(ordinal: "6")]
        public float N_62;
        [Column(ordinal: "7")]
        public float N_63;
        [Column(ordinal: "8")]
        public float N_64;
        [Column(ordinal: "9")]
        public float N_65;
        [Column(ordinal: "10")]
        public float N_66;
        [Column(ordinal: "11")]
        public float N_67;
        [Column(ordinal: "12")]
        public float N_68;
        [Column(ordinal: "13")]
        public float N_69;
        [Column(ordinal: "14")]
        public float N_70;
        [Column(ordinal: "15")]
        public float N_71;
        [Column(ordinal: "16")]
        public float N_72;
        [Column(ordinal: "17")]
        public float N_73;
        [Column(ordinal: "18")]
        public float N_74;
        [Column(ordinal: "19")]
        public float N_75;
        [Column(ordinal: "20")]
        public float N_76;
        [Column(ordinal: "21")]
        public float N_77;
        [Column(ordinal: "22")]
        public float N_78;
        [Column(ordinal: "23")]
        public float N_79;

        [Column(ordinal: "24", "Label")]
        public string Label;
    }

    public class MusicNotesPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Note;
    }
}

