/***************************************************************************
 *   Sinapse Neural Networking Tool         http://sinapse.googlecode.com  *
 *  ---------------------------------------------------------------------- *
 *   Copyright (C) 2006-2008 Cesar Roberto de Souza <cesarsouza@gmail.com> *
 *                                                                         *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 3 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using AForge.Math;

namespace Sinapse.Core.Sources
{
    public class NetworkSoundSource : NetworkDataSourceBase
    {

        public NetworkSoundSource(string title) : base(title)
        {

        }
        
        public override Matrix CreateVectors(NetworkDataSet set)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override DataView CreateDataView(NetworkDataSet set)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int InputsCount
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override int OutputsCount
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
    }
}