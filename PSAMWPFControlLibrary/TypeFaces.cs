/*
Polish System for Archivising Music WPF Control Library (PSAM WPF Control Library)
http://www.archiwistykamuzyczna.pl/index.php?article=download&lang=en#psamcontrollibrary

Copyright (c) 2010, Jacek Salamon
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list
  of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list
  of conditions and the following disclaimer in the documentation and/or other
  materials provided with the distribution.
* Neither the name of Jacek Salamon nor the names of contributors may be used to
  endorse or promote products derived from this software without specific prior
  written permission.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 

============================================================================================
Fugue Icons
Copyright (C) 2009 Yusuke Kamiyamane. All rights reserved.
The icons are licensed under a Creative Commons Attribution 3.0 license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace PSAMWPFControlLibrary
{
    public static class TypeFaces
    {
        public static Typeface MusicFont = new Typeface("Polihymnia");
        public static Typeface GraceNoteFont = new Typeface("Polihymnia");
        public static Typeface StaffFont = new Typeface("Polihymnia");
        public static Typeface LyricFont = new Typeface("Times New Roman");
        public static Typeface LyricFontBold = new Typeface("Times New Roman");
        public static Typeface MiscArticulationFont = new Typeface("Microsoft Sans Serif");
        public static Typeface DirectionFont = new Typeface("Microsoft Sans Serif");
        public static Typeface TrillFont = new Typeface("Times New Roman");
        public static Typeface TimeSignatureFont = new Typeface(new FontFamily("Microsoft Sans Serif"),
            FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
    }
}
