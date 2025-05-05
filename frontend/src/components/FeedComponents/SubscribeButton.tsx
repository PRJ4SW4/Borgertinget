// src/components/SubscribeButton.tsx
import React, { useState, useEffect } from 'react';
import { subscribe, unsubscribe } from '../../services/tweetService';

// Definer hvilke props komponenten skal modtage
interface SubscribeButtonProps {
  politicianTwitterId: number | null; // Det ID, som subscribe/unsubscribe API'et bruger (IKKE Aktor ID)
  initialIsSubscribed: boolean;       // Er brugeren subscribed, når komponenten vises?
  // Valgfri funktion der kaldes EFTER et succesfuldt subscribe/unsubscribe API-kald
  onSubscriptionChange?: (newStatus: boolean) => void;
}

const SubscribeButton: React.FC<SubscribeButtonProps> = ({
  politicianTwitterId,
  initialIsSubscribed,
  onSubscriptionChange
}) => {
  // Lokal state til at styre knappens udseende og deaktivering
  const [isSubscribed, setIsSubscribed] = useState<boolean>(initialIsSubscribed);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false); // Bruges til loading/disabled state
  const [error, setError] = useState<string | null>(null);         // Til at vise fejl under knappen

  useEffect(() => {
    setIsSubscribed(initialIsSubscribed);
  }, [initialIsSubscribed]);

  // Funktion der kaldes, når der klikkes på knappen
  const handleToggleSubscription = async () => {
    // Gør intet hvis ID mangler, eller hvis vi allerede er i gang
    if (politicianTwitterId === null || isSubmitting) {
        console.warn("Subscribe/Unsubscribe attempt ignored: missing ID or already submitting.");
        return;
    }

    setIsSubmitting(true); // Start loading
    setError(null);      // Nulstil fejl

    try {
      if (isSubscribed) {
        // --- Forsøg Unsubscribe ---
        console.log(`Attempting to unsubscribe from ${politicianTwitterId}`);
        await unsubscribe(politicianTwitterId);
        console.log(`Successfully unsubscribed (API call)`);
        setIsSubscribed(false); // Opdater knappen lokalt med det samme
        onSubscriptionChange?.(false); // Kald callback for at notificere forælder
      } else {
        // --- Forsøg Subscribe ---
        console.log(`Attempting to subscribe to ${politicianTwitterId}`);
        await subscribe(politicianTwitterId);
        console.log(`Successfully subscribed (API call)`);
        setIsSubscribed(true); // Opdater knappen lokalt med det samme
        onSubscriptionChange?.(true); // Kald callback for at notificere forælder
      }
    } catch (err: unknown) {
      console.error("Subscription toggle failed:", err);
      // Sæt fejlbesked og NULSTIL lokal state til den oprindelige status,
      // da API-kaldet fejlede
      setError(err instanceof Error ? err.message : "Ukendt fejl");
      setIsSubscribed(initialIsSubscribed); // Gå tilbage til den status, vi fik fra forælderen
    } finally {
      setIsSubmitting(false); // Stop altid loading
    }
  };

  // Hvis vi ikke har et ID at arbejde med (f.eks. fordi opslag fejer), vises intet
  if (politicianTwitterId === null) {
    return null; // Eller en disabled knap med en besked
  }

  // Render selve knappen
  return (
    <div className="subscribe-button-wrapper">
      <button
        onClick={handleToggleSubscription}
        disabled={isSubmitting} // Deaktiver mens API-kald kører
        className={`subscribe-toggle-button ${isSubscribed ? 'subscribed' : 'not-subscribed'}`}
      >
        {isSubmitting ? 'Arbejder...' : (isSubscribed ? 'Abonnerer (Fjern)' : 'Abonner')}
      </button>
      {/* Vis fejl hvis API-kaldet fejlede */}
      {error && <div className="subscribe-error">{error}</div>}
    </div>
  );
};

export default SubscribeButton;