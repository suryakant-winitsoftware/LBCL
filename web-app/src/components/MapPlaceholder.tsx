'use client'
import React from 'react'
import { colors } from '../styles/colors'

interface MapPlaceholderProps {
  salesmen: any[]
  selectedSalesman: string
  date: string
}

export const MapPlaceholder: React.FC<MapPlaceholderProps> = ({ salesmen, selectedSalesman, date }) => {
  return (
    <div style={{
      width: '100%',
      height: '600px',
      backgroundColor: colors.background.secondary,
      border: `2px solid ${colors.gray[200]}`,
      borderRadius: '8px',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '40px'
    }}>
      <div style={{ textAlign: 'center', maxWidth: '500px' }}>
        <div style={{ fontSize: '48px', marginBottom: '20px' }}>🗺️</div>
        <h3 style={{ fontSize: '24px', fontWeight: '600', color: colors.gray[900], marginBottom: '16px' }}>
          Map View Placeholder
        </h3>
        <p style={{ fontSize: '16px', color: colors.gray[600], marginBottom: '24px' }}>
          Google Maps will appear here once the API key is configured in Google Cloud Console.
        </p>

        <div style={{
          backgroundColor: colors.background.primary,
          padding: '20px',
          borderRadius: '8px',
          border: `1px solid ${colors.gray[200]}`,
          textAlign: 'left'
        }}>
          <h4 style={{ fontSize: '14px', fontWeight: '600', color: colors.gray[800], marginBottom: '12px' }}>
            Current Selection:
          </h4>
          <ul style={{ margin: 0, paddingLeft: '20px', color: colors.gray[600], fontSize: '14px' }}>
            <li>Date: {date}</li>
            <li>Salesman: {selectedSalesman === 'all' ? 'All Salesmen' : selectedSalesman}</li>
            <li>Active Salesmen: {salesmen.length}</li>
          </ul>
        </div>

        <div style={{
          marginTop: '24px',
          padding: '16px',
          backgroundColor: colors.warning.light,
          borderRadius: '6px',
          fontSize: '13px',
          color: colors.warning.dark
        }}>
          <strong>Note:</strong> To enable the map, configure your Google Maps API key with the Maps JavaScript API enabled.
        </div>
      </div>
    </div>
  )
}