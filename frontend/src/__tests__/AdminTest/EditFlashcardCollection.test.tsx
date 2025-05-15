import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, beforeEach, expect } from "vitest";
import EditFlashcardCollection from "../../components/AdminPages/EditFlashcardCollection";
import { mockLocalStorage, mockAlert, mockedAxios } from "../testMocks";
import { BrowserRouter } from "react-router-dom";

// Sample mock data for the tests
const mockTitles = ["Serie A", "Serie B"];
const mockCollection = {
  collectionId: 1,
  title: "Serie A",
  description: "Beskrivelse A",
  flashcards: [
    {
      flashcardId: 1,
      frontContentType: "Text",
      frontText: "Front A",
      frontImagePath: null,
      backContentType: "Text",
      backText: "Back A",
      backImagePath: null,
    },
  ],
};

describe("EditFlashcardCollection component", () => {
  // Runs before each test
  beforeEach(() => {
    mockLocalStorage(); // Mock localStorage
    mockAlert(); // Mock alert() calls
    localStorage.setItem("jwt", "fake-token"); // Set a dummy token

    // Reset axios mocks
    mockedAxios.get.mockReset();
    mockedAxios.put.mockReset();
  });

  it("fetches and displays all titles on mount", async () => {
    mockedAxios.get.mockResolvedValueOnce({ data: mockTitles }); // Mock GET /titles

    render(
      <BrowserRouter>
        <EditFlashcardCollection />
      </BrowserRouter>
    );

    // Check that the titles are shown
    for (const title of mockTitles) {
      expect(await screen.findByText(title)).toBeInTheDocument();
    }
  });

  it("loads a flashcard collection on title click", async () => {
    // First GET for titles, then GET for collection
    mockedAxios.get.mockResolvedValueOnce({ data: mockTitles }).mockResolvedValueOnce({ data: mockCollection });

    render(
      <BrowserRouter>
        <EditFlashcardCollection />
      </BrowserRouter>
    );

    // Click on the title to load the collection
    const titleButton = await screen.findByText("Serie A");
    fireEvent.click(titleButton);

    // Check if collection data is displayed
    expect(await screen.findByDisplayValue("Serie A")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Beskrivelse A")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Front A")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Back A")).toBeInTheDocument();
  });

  it("updates flashcard collection and shows success alert", async () => {
    // Mock fetching and saving the collection
    mockedAxios.get.mockResolvedValueOnce({ data: mockTitles }).mockResolvedValueOnce({ data: mockCollection });
    mockedAxios.put.mockResolvedValueOnce({ status: 200 });

    render(
      <BrowserRouter>
        <EditFlashcardCollection />
      </BrowserRouter>
    );

    // Load collection by clicking a title
    const titleButton = await screen.findByText("Serie A");
    fireEvent.click(titleButton);

    // Change the front text of the flashcard
    const frontInput = await screen.findByDisplayValue("Front A");
    fireEvent.change(frontInput, { target: { value: "Updated Front" } });

    // Click the save button
    fireEvent.click(screen.getByText("Rediger!"));

    // Wait for the PUT request and alert
    await waitFor(() => {
      // Check that PUT was called with correct URL and data
      expect(mockedAxios.put).toHaveBeenCalledWith(
        "/api/administrator/UpdateFlashcardCollection/1",
        expect.objectContaining({
          title: "Serie A",
          flashcards: expect.arrayContaining([
            expect.objectContaining({
              flashcardId: 1,
              frontContentType: "Text",
              frontText: "Updated Front", // Check for the updated value
              frontImagePath: null,
              backContentType: "Text",
              backText: "Back A",
              backImagePath: null,
            }),
          ]),
        }),
        expect.objectContaining({
          headers: expect.objectContaining({
            "Content-Type": "application/json", // Assuming JSON content type
            Authorization: "Bearer fake-token",
          }),
        })
      );

      // Check that the success alert is shown
      expect(window.alert).toHaveBeenCalledWith("Flashcard serien er redigeret!");
    });
  });
});
