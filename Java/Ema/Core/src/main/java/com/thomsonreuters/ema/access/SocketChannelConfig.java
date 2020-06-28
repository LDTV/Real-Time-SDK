///*|-----------------------------------------------------------------------------
// *|            This source code is provided under the Apache 2.0 license      --
// *|  and is provided AS IS with no warranty or guarantee of fit for purpose.  --
// *|                See the project's LICENSE.md for details.                  --
// *|           Copyright (C) 2019 Refinitiv. All rights reserved.            --
///*|-----------------------------------------------------------------------------

package com.thomsonreuters.ema.access;

import com.thomsonreuters.upa.transport.ConnectionTypes;

class SocketChannelConfig extends ChannelConfig
{
	String				hostName;
	String				serviceName;
	boolean				tcpNodelay;
	boolean				directWrite;
	
	SocketChannelConfig() 
	{
		 clear();
	}

	@Override
	void clear() 
	{
		super.clear();
		
		rsslConnectionType = ConnectionTypes.SOCKET;	
		hostName = ActiveConfig.DEFAULT_HOST_NAME;
		serviceName = ActiveConfig.defaultServiceName;
		tcpNodelay = ActiveConfig.DEFAULT_TCP_NODELAY;
		directWrite = ActiveConfig.DEFAULT_DIRECT_SOCKET_WRITE;
	}
}
