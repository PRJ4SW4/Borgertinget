// Fil: src/hooks/useDailyPhoto.ts
import { useState, useEffect } from "react";
import { PhotoDto } from "../types/polidleTypes";
import { getPhotoOfTheDay } from "../services/polidleApi";

interface UseDailyPhotoResult {
  photoData: PhotoDto | null;
  isLoading: boolean;
  error: string | null;
  retry: () => void; // Function to manually retry fetching
}

/**
 * Custom hook to fetch the daily Polidle photo.
 */
export function useDailyPhoto(): UseDailyPhotoResult {
  const [photoData, setPhotoData] = useState<PhotoDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [retryCount, setRetryCount] = useState(0); // State to trigger refetch

  useEffect(() => {
    const fetchPhoto = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getPhotoOfTheDay();
        setPhotoData(data);
      } catch (err: any) {
        console.error("Fetch photo error in hook:", err);
        setError(err.message || "Ukendt fejl ved hentning af foto.");
        setPhotoData(null); // Clear data on error
      } finally {
        setIsLoading(false);
      }
    };

    fetchPhoto();
  }, [retryCount]); // Refetch when retryCount changes

  const retry = () => {
    setRetryCount((prev) => prev + 1); // Increment retryCount to trigger useEffect
  };

  return { photoData, isLoading, error, retry };
}
