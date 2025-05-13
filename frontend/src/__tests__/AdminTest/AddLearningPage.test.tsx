import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import AddLearningPage from "../../components/AdminPages/AddLearningPage";
import * as ApiService from "../../services/ApiService";
import { mockNavigate, mockGetItem } from "../testMocks"; // Adjust path as needed

// ⚠️ Make sure path matches import!
vi.mock("../services/ApiService", async () => {
  const actual = await vi.importActual("../services/ApiService");
  return {
    ...actual,
    fetchPagesStructure: vi.fn(),
  };
});

describe("AddLearningPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetItem.mockReturnValue("fake-jwt-token");

    // Properly type cast mock
    (ApiService.fetchPagesStructure as Mock).mockResolvedValue([
      {
        id: 1,
        title: "Parent Page 1",
        parentPageId: null,
        displayOrder: 1,
        hasChildren: false,
      },
      {
        id: 2,
        title: "Parent Page 2",
        parentPageId: null,
        displayOrder: 2,
        hasChildren: false,
      },
    ]);

    (global.fetch as Mock).mockResolvedValue({
      ok: true,
      json: async () => ({ id: 100, title: "New Page" }),
      text: async () => "Success",
    });
  });

  it("renders the form correctly", async () => {
    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    expect(screen.getByText("Opret Læringsside")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText("Indhold (Markdown understøttet)")
    ).toBeInTheDocument();
    expect(screen.getByText("(Ingen overordnet side)")).toBeInTheDocument();
    expect(screen.getByText("Gem Side")).toBeInTheDocument();

    await waitFor(() => {
      expect(ApiService.fetchPagesStructure).toHaveBeenCalled();
    });

    expect(screen.getByText("Parent Page 1")).toBeInTheDocument();
  });

  it("updates title and content fields on input", () => {
    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    const titleInput = screen.getByPlaceholderText("Titel") as HTMLInputElement;
    fireEvent.change(titleInput, { target: { value: "Test Title" } });
    expect(titleInput.value).toBe("Test Title");

    const contentTextarea = screen.getByPlaceholderText(
      "Indhold (Markdown understøttet)"
    ) as HTMLTextAreaElement;
    fireEvent.change(contentTextarea, { target: { value: "Test Content" } });
    expect(contentTextarea.value).toBe("Test Content");
  });

  it("selects a parent page", async () => {
    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText("Parent Page 1")).toBeInTheDocument();
    });

    const parentSelect = screen.getByDisplayValue(
      "(Ingen overordnet side)"
    ) as HTMLSelectElement;
    fireEvent.change(parentSelect, { target: { value: "1" } });
    expect(parentSelect.value).toBe("1");
  });

  it("shows an alert if title is empty on submit", async () => {
    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    const saveButton = screen.getByText("Gem Side");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Titel må ikke være tom.");
    });

    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it("submits the form and navigates on success", async () => {
    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Titel"), {
      target: { value: "New Page Title" },
    });
    fireEvent.change(
      screen.getByPlaceholderText("Indhold (Markdown understøttet)"),
      { target: { value: "New Page Content" } }
    );

    await waitFor(() => {
      expect(screen.getByText("Parent Page 1")).toBeInTheDocument();
    });

    const parentSelect = screen.getByDisplayValue(
      "(Ingen overordnet side)"
    ) as HTMLSelectElement;
    fireEvent.change(parentSelect, { target: { value: "1" } });

    fireEvent.click(screen.getByText("Gem Side"));

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith("/api/pages", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: "Bearer fake-jwt-token",
        },
        body: JSON.stringify({
          title: "New Page Title",
          content: "New Page Content",
          parentPageId: 1,
          displayOrder: 1,
        }),
      });
    });

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Læringsside oprettet!");
      expect(mockNavigate).toHaveBeenCalledWith("/admin/Laering");
    });
  });

  it("shows an alert on API error during submission", async () => {
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: false,
      text: async () => "Server error",
    });

    render(
      <BrowserRouter>
        <AddLearningPage />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Titel"), {
      target: { value: "Error Page" },
    });
    fireEvent.click(screen.getByText("Gem Side"));

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalled();
      expect(window.alert).toHaveBeenCalledWith(
        "Fejl under oprettelse af side."
      );
      expect(mockNavigate).not.toHaveBeenCalled();
    });
  });
});
