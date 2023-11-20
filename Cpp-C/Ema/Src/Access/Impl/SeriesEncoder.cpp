/*|-----------------------------------------------------------------------------
 *|            This source code is provided under the Apache 2.0 license      --
 *|  and is provided AS IS with no warranty or guarantee of fit for purpose.  --
 *|                See the project's LICENSE.md for details.                  --
 *|           Copyright (C) 2019 Refinitiv. All rights reserved.            --
 *|-----------------------------------------------------------------------------
 */

#include "SeriesEncoder.h"
#include "ExceptionTranslator.h"
#include "StaticDecoder.h"
#include "Decoder.h"
#include "Series.h"
#include "OmmInvalidUsageException.h"

using namespace refinitiv::ema::access;

SeriesEncoder::SeriesEncoder() :
 _rsslSeries(),
 _rsslSeriesEntry(),
 _emaLoadType( DataType::NoDataEnum ),
 _containerInitialized( false ),
 _internalContainerCompleted( NULL )
{
}

SeriesEncoder::~SeriesEncoder()
{
}

void SeriesEncoder::clear()
{
	Encoder::releaseEncIterator();

	rsslClearSeries( &_rsslSeries );
	rsslClearSeriesEntry( &_rsslSeriesEntry );

	_emaLoadType = DataType::NoDataEnum;

	_containerInitialized = false;

	_internalContainerCompleted = NULL;
}

void SeriesEncoder::initEncode( UInt8 rsslDataType, DataType::DataTypeEnum emaDataType )
{
	if ( !_rsslSeries.containerType )
	{
		_rsslSeries.containerType = rsslDataType;
		_emaLoadType = emaDataType;
	}
	else if ( _rsslSeries.containerType != rsslDataType )
	{
		EmaString temp( "Attempt to add an entry with a DataType different than summaryData's DataType. Passed in ComplexType has DataType of " );
		temp += DataType( emaDataType ).toString();
		temp += EmaString( " while the expected DataType is " );
		temp += DataType( _emaLoadType );
		throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
		return;
	}

	RsslRet retCode = rsslEncodeSeriesInit( &(_pEncodeIter->_rsslEncIter), &_rsslSeries, 0, 0 );

	while ( retCode == RSSL_RET_BUFFER_TOO_SMALL )
	{
		retCode = rsslEncodeSeriesComplete( &(_pEncodeIter->_rsslEncIter), RSSL_FALSE );

		_pEncodeIter->reallocate();

		retCode = rsslEncodeSeriesInit( &(_pEncodeIter->_rsslEncIter), &_rsslSeries, 0, 0 );
	}

	if ( retCode < RSSL_RET_SUCCESS )
	{
		EmaString temp( "Failed to initialize Series encoding. Reason='" );
		temp.append( rsslRetCodeToString( retCode ) ).append( "'. " );
		throwIueException( temp, retCode );
	}

	_containerInitialized = true;
}

void SeriesEncoder::addEncodedEntry( const char* methodName, const RsslBuffer& rsslBuffer )
{
	_rsslSeriesEntry.encData = rsslBuffer;

	RsslRet retCode = rsslEncodeSeriesEntry( &_pEncodeIter->_rsslEncIter, &_rsslSeriesEntry );
	while ( retCode == RSSL_RET_BUFFER_TOO_SMALL )
	{
		_pEncodeIter->reallocate();

		retCode = rsslEncodeSeriesEntry( &_pEncodeIter->_rsslEncIter, &_rsslSeriesEntry );
	}

	if ( retCode < RSSL_RET_SUCCESS )
	{
		EmaString temp( "Failed to " );
		temp.append( methodName ).append( " while encoding Series. Reason='" );
		temp.append( rsslRetCodeToString( retCode ) ).append( "'. " );
		throwIueException( temp, retCode );
	}
}

void SeriesEncoder::startEncodingEntry( const char* methodName )
{
	_rsslSeriesEntry.encData.data = 0;
	_rsslSeriesEntry.encData.length = 0;

	RsslRet retCode = rsslEncodeSeriesEntryInit( &_pEncodeIter->_rsslEncIter, &_rsslSeriesEntry, 0 );
	while ( retCode == RSSL_RET_BUFFER_TOO_SMALL )
	{
		_pEncodeIter->reallocate();

		retCode = rsslEncodeSeriesEntryInit( &_pEncodeIter->_rsslEncIter, &_rsslSeriesEntry, 0 );
	}

	if ( retCode < RSSL_RET_SUCCESS )
	{
		EmaString temp( "Failed to start encoding entry in Series::" );
		temp.append( methodName ).append( ". Reason='" );
		temp.append( rsslRetCodeToString( retCode ) ).append( "'. " );
		throwIueException( temp, retCode );
	}
}

void SeriesEncoder::endEncodingEntry() const
{
	RsslRet retCode = rsslEncodeSeriesEntryComplete( &_pEncodeIter->_rsslEncIter, RSSL_TRUE );
	/* Reallocate does not need here. The data is placed in already allocated memory */

	if ( retCode < RSSL_RET_SUCCESS )
	{
		EmaString temp( "Failed to end encoding entry in Series. Reason='" );
		temp.append( rsslRetCodeToString( retCode ) ).append( "'. " );
		throwIueException( temp, retCode );
	}
}

void SeriesEncoder::verifyPayLoadCompleted(const Encoder& enc, const UInt8& rsslData )
{
	if (_internalContainerCompleted)
	{
		if (*_internalContainerCompleted == false)
		{
			EmaString temp("Attemp to add new container to the FieldList while complete() was not called for previously added container: ");
			temp.append(rsslDataTypeToString(_rsslSeries.containerType));
			_internalContainerCompleted = NULL;
			throwIueException(temp, OmmInvalidUsageException::InvalidArgumentEnum);
			return;
		}
	}
	if ( rsslData == RSSL_DT_MSG ||
		 rsslData == RSSL_DT_XML ||
		 rsslData == RSSL_DT_OPAQUE ||
		 rsslData == RSSL_DT_ANSI_PAGE )
		_internalContainerCompleted = NULL;
	else
		_internalContainerCompleted = const_cast<Encoder&>(enc).isCompletePtr();
}

void SeriesEncoder::add( const ComplexType& complexType )
{
	if ( _containerComplete )
	{
		EmaString temp( "Attempt to add an entry after complete() was called." );
		throwIueException( temp, OmmInvalidUsageException::InvalidOperationEnum );
		return;
	}

	const Encoder& enc = complexType.getEncoder();

	UInt8 rsslDataType = enc.convertDataType( complexType.getDataType() );

	if ( !hasEncIterator() )
	{
		acquireEncIterator();

		initEncode( rsslDataType, complexType.getDataType() );
	}
	else if ( _rsslSeries.containerType != rsslDataType )
	{
		EmaString temp( "Attempt to add an entry with a different DataType. Passed in ComplexType has DataType of " );
		temp += DataType( complexType.getDataType() ).toString();
		temp += EmaString( " while the expected DataType is " );
		temp += DataType( _emaLoadType );
		throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
		return;
	}

	if ( complexType.hasEncoder() && enc.ownsIterator() )
	{
		if( enc.isComplete() )
		{
			verifyPayLoadCompleted( enc, rsslDataType );
			addEncodedEntry("add()", enc.getRsslBuffer());
		}
		else
		{
			EmaString temp( "Attempt to add() a ComplexType while complete() was not called on this ComplexType." );
			throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
			return;
		}
	}
	else if ( complexType.hasDecoder() )
	{
		addEncodedEntry( "add()", const_cast<ComplexType&>( complexType ).getDecoder().getRsslBuffer() );
	}
	else
	{
		if ( rsslDataType == RSSL_DT_MSG )
		{
			EmaString temp( "Attempt to pass in an empty message while it is not supported." );
			throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
			return;
		}

		verifyPayLoadCompleted( enc, rsslDataType );

		passEncIterator( const_cast<Encoder&>( enc ) );
		startEncodingEntry( "add()" );
	}
}

void SeriesEncoder::add()
{
	if (_containerComplete)
	{
		EmaString temp("Attempt to add an entry after complete() was called.");
		throwIueException( temp, OmmInvalidUsageException::InvalidOperationEnum );
		return;
	}

	UInt8 rsslDataType = RSSL_DT_NO_DATA;

	if (!hasEncIterator())
	{
		acquireEncIterator();

		initEncode(rsslDataType, DataType::NoDataEnum);
	}
	else if (_rsslSeries.containerType != rsslDataType)
	{
		EmaString temp("Attempt to add an entry with a different DataType. Encode DataType as ");
		temp += DataType(DataType::NoDataEnum).toString();
		temp += EmaString(" while the expected DataType is ");
		temp += DataType( _emaLoadType );
		throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
		return;
	}

	RsslBuffer rsslBuffer;
	rsslClearBuffer(&rsslBuffer);
	addEncodedEntry("add()", rsslBuffer);
}

void SeriesEncoder::complete()
{
	if ( _containerComplete ) return;

	if ( _internalContainerCompleted )
	{
		if ( *_internalContainerCompleted == false )
		{
			/*If an internal container is not completed. Internal container empty.*/
			EmaString temp( "Attemp to complete Series while complete() was not called for internal container: " );
			temp.append( rsslDataTypeToString( _rsslSeries.containerType ) );
			_internalContainerCompleted = NULL;
			throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
			return;
		}
	}

	if (!hasEncIterator())
	{
		acquireEncIterator();

		initEncode(convertDataType( _emaLoadType ), _emaLoadType);
	}

	RsslRet retCode = rsslEncodeSeriesComplete( &(_pEncodeIter->_rsslEncIter), RSSL_TRUE );

	if ( retCode < RSSL_RET_SUCCESS )
	{
		EmaString temp( "Failed to complete Series encoding. Reason='" );
		temp.append( rsslRetCodeToString( retCode ) ).append( "'. " );
		throwIueException( temp, retCode );
		return;
	}

	_pEncodeIter->setEncodedLength( rsslGetEncodedBufferLength( &(_pEncodeIter->_rsslEncIter) ) );

	if ( !ownsIterator() && _iteratorOwner )
		_iteratorOwner->endEncodingEntry();

	_containerComplete = true;
}

void SeriesEncoder::totalCountHint( UInt32 totalCountHint )
{
	if ( !_containerInitialized )
	{
		rsslSeriesApplyHasTotalCountHint( &_rsslSeries );
		_rsslSeries.totalCountHint = totalCountHint;
	}
	else
	{
		EmaString temp( "Invalid attempt to call totalCountHint() when container is initialized." );
		throwIueException( temp, OmmInvalidUsageException::InvalidOperationEnum );
	}
}

void SeriesEncoder::summaryData( const ComplexType& data )
{
	if ( !_containerInitialized )
	{
		const Encoder& enc = data.getEncoder();

		if ( data.hasEncoder() && enc.ownsIterator() )
		{
			if ( enc.isComplete() )
			{
				rsslSeriesApplyHasSummaryData( &_rsslSeries );
				_rsslSeries.encSummaryData = enc.getRsslBuffer();
			}
			else
			{
				EmaString temp( "Attempt to set summaryData() with a ComplexType while complete() was not called on this ComplexType." );
				throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
			}
		}
		else if ( data.hasDecoder() )
		{
			rsslSeriesApplyHasSummaryData( &_rsslSeries );
			_rsslSeries.encSummaryData = const_cast<ComplexType&>(data).getDecoder().getRsslBuffer();
		}
		else
		{
			EmaString temp( "Attempt to pass an empty ComplexType to summaryData() while it is not supported." );
			throwIueException( temp, OmmInvalidUsageException::InvalidArgumentEnum );
			return;
		}

		_emaLoadType = data.getDataType();
		_rsslSeries.containerType = enc.convertDataType( _emaLoadType );
	}
	else
	{
		EmaString temp( "Invalid attempt to call summaryData() when container is initialized." );
		throwIueException( temp, OmmInvalidUsageException::InvalidOperationEnum );
	}
}
