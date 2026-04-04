import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export function useGameHub(gameCode, handlers) {
  const connectionRef = useRef(null);
  const handlersRef = useRef(handlers);
  handlersRef.current = handlers;

  useEffect(() => {
    if (!gameCode) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .build();

    // Register all event handlers
    connection.on('PlayerJoined', (data) => handlersRef.current.onPlayerJoined?.(data));
    connection.on('GameStarted', () => handlersRef.current.onGameStarted?.());
    connection.on('RoundStarted', (data) => handlersRef.current.onRoundStarted?.(data));
    connection.on('CardSubmitted', (data) => handlersRef.current.onCardSubmitted?.(data));
    connection.on('JudgingPhase', (submissions) => handlersRef.current.onJudgingPhase?.(submissions));
    connection.on('RoundFinished', (data) => handlersRef.current.onRoundFinished?.(data));
    connection.on('GameFinished', (data) => handlersRef.current.onGameFinished?.(data));
    connection.on('PlayerLeft', (data) => handlersRef.current.onPlayerLeft?.(data));

    connectionRef.current = connection;

    connection.start()
      .then(() => connection.invoke('JoinGameGroup', gameCode))
      .catch(console.error);

    return () => {
      connection.invoke('LeaveGameGroup', gameCode).catch(() => {});
      connection.stop();
    };
  }, [gameCode]);

  const invoke = useCallback((method, ...args) => {
    return connectionRef.current?.invoke(method, ...args);
  }, []);

  return { invoke };
}
