import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, beforeEach, expect } from "vitest";
import DeleteFlashcardCollection from "../../components/AdminPages/DeleteFlashcardCollection";
import { mockLocalStorage, mockAlert, mockedAxios } from "../testMocks";
import { BrowserRouter } from "react-router-dom";

// Sample data for mocking
const mockTitles = ["Serie A", "Serie B"];
const mockCollection = {
  collectionId: 1,
  title: "Serie A",
  description: "Desc",
  flashcards: [],
};

describe("DeleteFlashcardCollection component", () => {
  beforeEach(() => {
    mockLocalStorage(); // Mock localStorage for token
    mockAlert(); // Prevent real alert popups
    localStorage.setItem("jwt", "fake-token");

    mockedAxios.get.mockReset();
    mockedAxios.delete.mockReset();
  });

  it("displays all flashcard series titles on mount", async () => {
    // Mock GET for titles
    mockedAxios.get.mockResolvedValueOnce({ data: mockTitles });

    render(
      <BrowserRouter>
        <DeleteFlashcardCollection />
      </BrowserRouter>
    );

    // All titles should be visible
    for (const title of mockTitles) {
      expect(await screen.findByText(title)).toBeInTheDocument();
    }
  });

  it("deletes a selected flashcard collection and updates the UI", async () => {
    // 1. First GET returns all titles
    // 2. Second GET returns the collection to delete
    // 3. DELETE request mocks deletion
    mockedAxios.get
      .mockResolvedValueOnce({ data: mockTitles }) // titles
      .mockResolvedValueOnce({ data: mockCollection }); // selected collection
    mockedAxios.delete.mockResolvedValueOnce({ status: 200 });

    render(
      <BrowserRouter>
        <DeleteFlashcardCollection />
      </BrowserRouter>
    );

    // Click on a title to trigger deletion
    const deleteButton = await screen.findByText("Serie A");
    fireEvent.click(deleteButton);

    // Expect correct GET and DELETE calls
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(
        "/api/administrator/GetFlashcardCollectionByTitle?title=Serie%20A",
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: "Bearer fake-token",
          }),
        })
      );

      expect(mockedAxios.delete).toHaveBeenCalledWith(
        "/api/administrator/DeleteFlashcardCollection?collectionId=1",
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: "Bearer fake-token",
          }),
        })
      );

      expect(window.alert).toHaveBeenCalledWith("Flashcard serie slettet!");
    });

    // The deleted title should no longer be in the DOM
    expect(screen.queryByText("Serie A")).not.toBeInTheDocument();
  });
});
