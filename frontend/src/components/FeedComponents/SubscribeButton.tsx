import React, { useState, useEffect } from 'react';
import { subscribe, unsubscribe } from '../../services/tweetService';

interface SubscribeButtonProps {
  politicianTwitterId: number | null; // Det ID, som subscribe/unsubscribe API'et bruger, altså hvilken politiker der abonneres på
  initialIsSubscribed: boolean;       // Er brugeren subscribed, når komponenten vises
  // Valgfri funktion der kaldes EFTER et succesfuldt subscribe/unsubscribe API-kald
  onSubscriptionChange?: (newStatus: boolean) => void;
}

const SubscribeButton: React.FC<SubscribeButtonProps> = ({
  politicianTwitterId,
  initialIsSubscribed,
  onSubscriptionChange
}) => {
  // Lokal state til at styre knappens udseende og deaktivering når der er subscribet/unsubscribed
  const [isSubscribed, setIsSubscribed] = useState<boolean>(initialIsSubscribed);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false); 
  const [error, setError] = useState<string | null>(null);         

  useEffect(() => {
    setIsSubscribed(initialIsSubscribed);
  }, [initialIsSubscribed]);

  const handleToggleSubscription = async () => {
    if (politicianTwitterId === null || isSubmitting) {
        console.warn("Subscribe/Unsubscribe attempt ignored: missing ID or already submitting.");
        return;
    }

    setIsSubmitting(true); 
    setError(null);    

    try {
      if (isSubscribed) {
        console.log(`Attempting to unsubscribe from ${politicianTwitterId}`);
        await unsubscribe(politicianTwitterId);
        console.log(`Successfully unsubscribed (API call)`);
        setIsSubscribed(false); 
        onSubscriptionChange?.(false); 
      } else {
        // --- Forsøg Subscribe ---
        console.log(`Attempting to subscribe to ${politicianTwitterId}`);
        await subscribe(politicianTwitterId);
        console.log(`Successfully subscribed (API call)`);
        setIsSubscribed(true); 
        onSubscriptionChange?.(true); 
      }
    } catch (err: unknown) {
      console.error("Subscription toggle failed:", err);
      
      setError(err instanceof Error ? err.message : "Ukendt fejl");
      setIsSubscribed(initialIsSubscribed); 
    } finally {
      setIsSubmitting(false); 
    }
  };

  
  if (politicianTwitterId === null) {
    return null; 
  }

  return (
    <div className="subscribe-button-wrapper">
      <button
        onClick={handleToggleSubscription}
        disabled={isSubmitting} 
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