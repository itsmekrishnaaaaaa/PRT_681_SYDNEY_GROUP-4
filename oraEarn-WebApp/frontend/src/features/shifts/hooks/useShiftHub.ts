import * as signalR from '@microsoft/signalr';
import { useEffect, useRef } from 'react';
import { getAccessToken } from '@/shared/api/client';
import { HUB_URL } from '@/shared/api/config';
import type { Shift } from '@/shared/types';

type ShiftHandlers = {
  onCreated: (shift: Shift) => void;
  onUpdated: (shift: Shift) => void;
  onDeleted: (shiftId: string) => void;
};

export function useShiftHub(handlers: ShiftHandlers, enabled = true) {
  const handlersRef = useRef(handlers);
  handlersRef.current = handlers;

  useEffect(() => {
    if (!enabled || !getAccessToken()) {
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ShiftCreated', (shift: Shift) => handlersRef.current.onCreated(shift));
    connection.on('ShiftUpdated', (shift: Shift) => handlersRef.current.onUpdated(shift));
    connection.on('ShiftDeleted', (shiftId: string) => handlersRef.current.onDeleted(shiftId));

    void connection.start();

    return () => {
      void connection.stop();
    };
  }, [enabled]);
}
