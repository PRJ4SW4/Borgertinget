import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, beforeEach, expect } from "vitest";
import CreateFlashcardCollection from "../../components/AdminPages/AddFlashcardCollection";
import { BrowserRouter } from "react-router-dom";
import { mockLocalStorage, mockedAxios, mockAlert } from "../testMocks";

describe("CreateFlashcardCollection component", () => {
  beforeEach(() => {
    mockLocalStorage();
    localStorage.setItem("jwt", "fake-jwt-token"); // Ensure jwt exists for auth

    mockedAxios.post.mockReset(); // Clean up between tests
    mockAlert(); // Avoid actual alerts
  });

  it("renders without crashing", () => {
    render(
      <BrowserRouter>
        <CreateFlashcardCollection />
      </BrowserRouter>
    );
    expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Beskrivelse")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Spørgsmål")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Svar")).toBeInTheDocument();
  });

  it("adds a new text flashcard when button is clicked", () => {
    render(
      <BrowserRouter>
        <CreateFlashcardCollection />
      </BrowserRouter>
    );

    const button = screen.getByText("Tilføj Flashcard");
    fireEvent.click(button);

    expect(screen.getAllByPlaceholderText("Spørgsmål")).toHaveLength(2);
    expect(screen.getAllByPlaceholderText("Svar")).toHaveLength(2);
  });

  it("shows validation alert if form fields are empty", async () => {
    render(
      <BrowserRouter>
        <CreateFlashcardCollection />
      </BrowserRouter>
    );

    fireEvent.click(screen.getByText("Opret!"));

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Felt mangler at blive udfyldt!");
    });

    expect(mockedAxios.post).not.toHaveBeenCalled();
  });

  it("submits valid form and resets fields", async () => {
    mockedAxios.post.mockResolvedValueOnce({ data: { message: "Success" } });

    render(
      <BrowserRouter>
        <CreateFlashcardCollection />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Titel"), {
      target: { value: "Min samling" },
    });
    fireEvent.change(screen.getByPlaceholderText("Beskrivelse"), {
      target: { value: "En testbeskrivelse" },
    });
    fireEvent.change(screen.getByPlaceholderText("Spørgsmål"), {
      target: { value: "Hvad er AI?" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svar"), {
      target: { value: "Kunstig intelligens" },
    });

    fireEvent.click(screen.getByText("Opret!"));

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledWith(
        "/api/administrator/PostFlashcardCollection",
        expect.objectContaining({
          collectionId: 0, // Added expected collectionId
          title: "Min samling",
          description: "En testbeskrivelse",
          flashcards: expect.arrayContaining([
            expect.objectContaining({
              flashcardId: 0, // Added expected flashcardId
              frontContentType: "Text", // Added expected frontContentType
              frontText: "Hvad er AI?",
              frontImagePath: null, // Added expected frontImagePath
              backContentType: "Text", // Added expected backContentType
              backText: "Kunstig intelligens",
              backImagePath: null, // Added expected backImagePath
            }),
          ]),
        }),
        expect.objectContaining({
          headers: expect.objectContaining({
            "Content-Type": "application/json",
            Authorization: "Bearer fake-jwt-token",
          }),
        })
      );
      expect(window.alert).toHaveBeenCalledWith("Flashcard serie er oprettet!!");
    });

    expect(screen.getByPlaceholderText("Titel")).toHaveValue("");
    expect(screen.getByPlaceholderText("Beskrivelse")).toHaveValue("");
  });
});
