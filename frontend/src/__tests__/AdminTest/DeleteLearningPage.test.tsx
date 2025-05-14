import { render, screen, fireEvent, waitFor, within } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import DeleteLearningPage from "../../components/AdminPages/DeleteLearningPage";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

// Import the function to be mocked and types for mock data
import { fetchPagesStructure } from "../../services/ApiService";
import type { PageSummaryDto, PageDetailDto as ApiPageDetailDto } from "../../types/pageTypes";

// Mock the ApiService module
vi.mock("../../services/ApiService", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../services/ApiService")>();
  return {
    ...actual,
    fetchPagesStructure: vi.fn(), // Mock the specific function
  };
});

// Cast the mocked function for type safety and to access mock methods
const mockedFetchPagesStructure = fetchPagesStructure as Mock<() => Promise<PageSummaryDto[]>>;

const mockPagesSummary: PageSummaryDto[] = [
  {
    id: 1,
    title: "Page to Delete",
    parentPageId: null,
    displayOrder: 1,
    hasChildren: false,
  },
  {
    id: 2,
    title: "Another Page",
    parentPageId: null,
    displayOrder: 2,
    hasChildren: false,
  },
];

const mockPageDetail: ApiPageDetailDto = {
  id: 1,
  title: "Page to Delete Title",
  content: "Page to Delete Content",
  parentPageId: null,
  previousSiblingId: null, // Added missing property
  nextSiblingId: null, // Added missing property
  associatedQuestions: [], // Ensure this matches the DTO structure
};

describe("DeleteLearningPage", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");

    // Mock for fetchPagesStructure (used in the first useEffect)
    mockedFetchPagesStructure.mockResolvedValue([...mockPagesSummary]);

    // Mock for axios.get (used in the second useEffect for page details)
    mockedAxios.get.mockImplementation(async (url: string) => {
      if (url === `/api/pages/${mockPageDetail.id}`) {
        return Promise.resolve({ data: { ...mockPageDetail } });
      }
      return Promise.reject(new Error(`mockedAxios.get: unhandled URL ${url}`));
    });

    // Mock for axios.delete (used in handleDelete)
    mockedAxios.delete.mockResolvedValue({ data: {} });

    // Reset mocks before each test
    mockedFetchPagesStructure.mockClear();
    mockedAxios.get.mockClear();
    mockedAxios.delete.mockClear();
    mockNavigate.mockClear();

    // Ensure window.confirm and window.alert are reset (if not globally handled or needing override)
    if (window.confirm as Mock) {
      (window.confirm as Mock).mockReset();
      (window.confirm as Mock).mockReturnValue(true); // Default to true
    }
    if (window.alert as Mock) {
      (window.alert as Mock).mockReset();
    }
  });

  it("renders correctly and fetches pages", async () => {
    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );

    expect(screen.getByText("Slet Læringsside")).toBeInTheDocument();
    expect(screen.getByText("-- Vælg en side --")).toBeInTheDocument();

    await waitFor(() => {
      // Assert that fetchPagesStructure was called for the initial list
      expect(mockedFetchPagesStructure).toHaveBeenCalledTimes(1);
    });

    // Check if options are populated from the mocked fetchPagesStructure
    mockPagesSummary.forEach((page) => {
      expect(screen.getByRole("option", { name: page.title })).toBeInTheDocument();
    });
  });

  it("does not render page details if no page is selected", () => {
    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );
    expect(screen.queryByLabelText("Titel")).not.toBeInTheDocument();
  });

  it("fetches and displays page details when a page is selected", async () => {
    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );

    // Wait for the initial page list to load and populate the select
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page to Delete" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", {
      name: /vælg side/i,
    }) as HTMLSelectElement;
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/pages/1", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
    });

    expect(screen.getByLabelText("Titel")).toHaveValue(mockPageDetail.title);
    expect(screen.getByLabelText("Indhold")).toHaveValue(mockPageDetail.content);
    // Ensure the delete button within the form is checked
    const form = screen.getByLabelText("Titel").closest("form");
    expect(within(form!).getByRole("button", { name: "Slet Side" })).toBeInTheDocument();
  });

  it("deletes the page and navigates on successful deletion after confirmation", async () => {
    (window.confirm as Mock).mockReturnValue(true); // Explicitly set for this test, though default is true

    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page to Delete" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      // Ensure the form's delete button is present
      const form = screen.getByLabelText("Titel").closest("form");
      expect(within(form!).getByRole("button", { name: "Slet Side" })).toBeInTheDocument();
    });

    const deleteButton = within(screen.getByLabelText("Titel").closest("form")!).getByRole("button", { name: "Slet Side" });
    fireEvent.click(deleteButton);

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne læringsside?");

    await waitFor(() => {
      expect(mockedAxios.delete).toHaveBeenCalledWith("/api/pages/1", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
    });

    expect(window.alert).toHaveBeenCalledWith("Siden er slettet.");
    expect(mockNavigate).toHaveBeenCalledWith("/admin/Laering");
  });

  it("does not delete the page if confirmation is cancelled", async () => {
    (window.confirm as Mock).mockReturnValue(false);

    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page to Delete" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      // Ensure the form's delete button is present
      const form = screen.getByLabelText("Titel").closest("form");
      expect(within(form!).getByRole("button", { name: "Slet Side" })).toBeInTheDocument();
    });

    const deleteButton = within(screen.getByLabelText("Titel").closest("form")!).getByRole("button", { name: "Slet Side" });
    fireEvent.click(deleteButton);

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne læringsside?");
    expect(mockedAxios.delete).not.toHaveBeenCalled();
    expect(window.alert).not.toHaveBeenCalled();
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it("shows an alert on API error during deletion", async () => {
    (window.confirm as Mock).mockReturnValue(true);
    mockedAxios.delete.mockRejectedValueOnce(new Error("Delete failed"));

    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page to Delete" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      // Ensure the form's delete button is present
      const form = screen.getByLabelText("Titel").closest("form");
      expect(within(form!).getByRole("button", { name: "Slet Side" })).toBeInTheDocument();
    });

    const deleteButton = within(screen.getByLabelText("Titel").closest("form")!).getByRole("button", { name: "Slet Side" });
    fireEvent.click(deleteButton);

    await waitFor(() => {
      expect(mockedAxios.delete).toHaveBeenCalled();
    });

    expect(window.alert).toHaveBeenCalledWith("Sletning fejlede.");
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
