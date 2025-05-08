/**
 * Converts a byte array (represented as an array of numbers) to a Base64 data URL.
 * @param byteArray The array of numbers representing bytes.
 * @param mimeType The MIME type of the image (default: 'image/png').
 * @returns A data URL string or a placeholder path if conversion fails.
 */
export function convertByteArrayToDataUrl(
  byteArray: number[] | undefined | null, // Allow undefined/null input
  mimeType = "image/png"
): string {
  const placeholder = "/placeholders/default_avatar.png"; // Path to your placeholder image in public folder

  if (!byteArray || byteArray.length === 0) {
    console.warn(
      "convertByteArrayToDataUrl received empty or null array, using placeholder."
    );
    return placeholder;
  }

  try {
    // Convert number array to Uint8Array
    const uint8Array = new Uint8Array(byteArray);

    // Convert Uint8Array to binary string
    let binaryString = "";
    uint8Array.forEach((byte) => {
      binaryString += String.fromCharCode(byte);
    });

    // Convert binary string to Base64
    const base64String = btoa(binaryString);

    return `data:${mimeType};base64,${base64String}`;
  } catch (error) {
    console.error("Error converting byte array to data URL:", error);
    return placeholder; // Return placeholder on error
  }
}

/**
 * Converts a Base64 string directly to a data URL.
 * Assumes the Base64 string is valid image data.
 * @param base64String The Base64 encoded image data.
 * @param mimeType The MIME type of the image (default: 'image/png').
 * @returns A data URL string or a placeholder path if input is invalid.
 */
export function convertBase64ToDataUrl(
  base64String: string | undefined | null,
  mimeType = "image/png"
): string {
  const placeholder = "/placeholders/default_avatar.png"; // Path to your placeholder image

  if (!base64String) {
    console.warn(
      "convertBase64ToDataUrl received empty or null string, using placeholder."
    );
    return placeholder;
  }
  // Basic check if it looks like Base64, might need more robust validation
  if (base64String.length % 4 !== 0 || /[^A-Z0-9+/=]/i.test(base64String)) {
    console.warn(
      "Input string doesn't look like valid Base64, using placeholder."
    );
    return placeholder;
  }

  return `data:${mimeType};base64,${base64String}`;
}
