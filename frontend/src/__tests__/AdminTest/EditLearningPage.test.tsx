import { render, screen, fireEvent, waitFor, within } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import EditLearningPage from "../../components/AdminPages/EditLearningPage";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

import { fetchPagesStructure } from "../../services/ApiService";
import type { PageSummaryDto, PageDetailDto as ApiPageDetailDto } from "../../types/pageTypes";

vi.mock("../../services/ApiService", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../services/ApiService")>();
  return {
    ...actual,
    fetchPagesStructure: vi.fn(),
  };
});

// Cast the mocked function for type safety and to access mock methods
const mockedFetchPagesStructure = fetchPagesStructure as Mock<() => Promise<PageSummaryDto[]>>;

const mockPagesSummary: PageSummaryDto[] = [
  {
    id: 1,
    title: "Page 1",
    parentPageId: null,
    displayOrder: 1,
    hasChildren: false,
  },
  {
    id: 2,
    title: "Page 2",
    parentPageId: 1,
    displayOrder: 1,
    hasChildren: false,
  },
  {
    id: 3,
    title: "Page 3",
    parentPageId: null,
    displayOrder: 2,
    hasChildren: true,
  },
];

const mockPageDetail: ApiPageDetailDto = {
  id: 1,
  title: "Page 1 Title",
  content: "Page 1 Content",
  parentPageId: null,
  previousSiblingId: null,
  nextSiblingId: null,
  associatedQuestions: [],
};

describe("EditLearningPage", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");

    mockedFetchPagesStructure.mockResolvedValue([...mockPagesSummary]);

    mockedAxios.get.mockImplementation(async (url: string) => {
      if (url === `/api/pages/${mockPageDetail.id}`) {
        return Promise.resolve({ data: { ...mockPageDetail } });
      }
      return Promise.reject(new Error(`mockedAxios.get: unhandled URL ${url}`));
    });

    mockedAxios.put.mockResolvedValue({ data: {} });

    mockedFetchPagesStructure.mockClear();
    mockedAxios.get.mockClear();
    mockedAxios.put.mockClear();
    mockNavigate.mockClear();

    if (window.alert as Mock) {
      (window.alert as Mock).mockReset();
    }
  });

  it("renders correctly and fetches pages", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );

    expect(screen.getByText("Rediger Læringsside")).toBeInTheDocument();
    expect(screen.getByText("-- Vælg side --")).toBeInTheDocument();

    await waitFor(() => {
      expect(mockedFetchPagesStructure).toHaveBeenCalledTimes(1);
    });

    mockPagesSummary.forEach((page) => {
      expect(screen.getByRole("option", { name: page.title })).toBeInTheDocument();
    });
  });

  it("does not render form if no page is selected", () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    expect(screen.queryByPlaceholderText("Titel")).not.toBeInTheDocument();
  });

  it("fetches and displays page details when a page is selected", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
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

    expect(screen.getByPlaceholderText("Titel")).toHaveValue(mockPageDetail.title);
    expect(screen.getByPlaceholderText("Markdown indhold")).toHaveValue(mockPageDetail.content);
    expect(screen.getByText("Gem Ændringer")).toBeInTheDocument();
  });

  it("updates form fields on input", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument();
    });

    const titleInput = screen.getByPlaceholderText("Titel") as HTMLInputElement;
    fireEvent.change(titleInput, { target: { value: "Updated Title" } });
    expect(titleInput.value).toBe("Updated Title");

    const contentTextarea = screen.getByPlaceholderText("Markdown indhold") as HTMLTextAreaElement;
    fireEvent.change(contentTextarea, { target: { value: "Updated Content" } });
    expect(contentTextarea.value).toBe("Updated Content");

    const parentSelect = screen.getByRole("combobox", {
      name: /overordnet side/i,
    }) as HTMLSelectElement;
    expect(within(parentSelect).getByText("Page 2")).toBeInTheDocument();
    expect(within(parentSelect).queryByText("Page 1")).not.toBeInTheDocument();
    fireEvent.change(parentSelect, { target: { value: "2" } });
    expect(parentSelect.value).toBe("2");
  });

  it("submits the form and navigates on successful update", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Titel")).toHaveValue(mockPageDetail.title);
    });

    const titleInput = screen.getByPlaceholderText("Titel");
    fireEvent.change(titleInput, { target: { value: "Final Title" } });

    const contentTextarea = screen.getByPlaceholderText("Markdown indhold");
    fireEvent.change(contentTextarea, { target: { value: "Final Content" } });

    const saveButton = screen.getByText("Gem Ændringer");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(mockedAxios.put).toHaveBeenCalledWith(
        "/api/pages/1",
        {
          id: 1,
          title: "Final Title",
          content: "Final Content",
          parentPageId: null,
          displayOrder: 1,
          associatedQuestions: [],
        },
        { headers: { Authorization: "Bearer fake-jwt-token" } }
      );
    });

    expect(window.alert).toHaveBeenCalledWith("Siden er opdateret!");
    expect(mockNavigate).toHaveBeenCalledWith("/admin/Laering");
  });

  it("shows an alert on API error during update", async () => {
    mockedAxios.put.mockRejectedValueOnce(new Error("Update failed"));
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument();
    });

    const saveButton = screen.getByText("Gem Ændringer");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(mockedAxios.put).toHaveBeenCalled();
    });

    expect(window.alert).toHaveBeenCalledWith("Opdatering fejlede.");
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it("can add and remove a question", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    await waitFor(() => expect(screen.getByText("Tilføj Spørgsmål")).toBeInTheDocument());
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    expect(screen.getByPlaceholderText("Indtast spørgsmål")).toBeInTheDocument();
    fireEvent.click(screen.getByText("Fjern Spørgsmål"));
    expect(screen.queryByPlaceholderText("Indtast spørgsmål")).not.toBeInTheDocument();
  });

  it("can change the question text", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    const questionTextarea = screen.getByPlaceholderText("Indtast spørgsmål");
    fireEvent.change(questionTextarea, { target: { value: "Hvad er 2+2?" } });
    expect(questionTextarea).toHaveValue("Hvad er 2+2?");
  });

  it("can add and remove answer options for a question", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    expect(screen.getAllByPlaceholderText("Svarsmulighed").length).toBe(2);
    fireEvent.click(screen.getByText("Tilføj Svarsmulighed"));
    expect(screen.getAllByPlaceholderText("Svarsmulighed").length).toBe(3);
    fireEvent.click(screen.getByText("Fjern Sidste Svar"));
    expect(screen.getAllByPlaceholderText("Svarsmulighed").length).toBe(2);
  });

  it("can change the answer option text", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    const optionInputs = screen.getAllByPlaceholderText("Svarsmulighed");
    fireEvent.change(optionInputs[0], { target: { value: "4" } });
    expect(optionInputs[0]).toHaveValue("4");
  });

  it("can mark an answer option as correct", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    const checkboxes = screen.getAllByRole("checkbox");
    expect(checkboxes[0]).not.toBeChecked();
    fireEvent.click(checkboxes[0]);
    expect(checkboxes[0]).toBeChecked();
  });

  it("shows alert if trying to remove option when none exist", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    fireEvent.click(screen.getByText("Tilføj Spørgsmål"));
    // Remove all options
    const removeOptionBtn = screen.getByText("Fjern Sidste Svar");
    fireEvent.click(removeOptionBtn);
    fireEvent.click(removeOptionBtn);
    // Now try to remove again, should alert
    fireEvent.click(removeOptionBtn);
    expect(window.alert).toHaveBeenCalledWith("Der er ingen svarmuligheder at fjerne.");
  });

  it("can change the display order", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    await waitFor(() => expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument());
    const displayOrderInput = screen.getByLabelText("Visningsrækkefølge") as HTMLInputElement;
    fireEvent.change(displayOrderInput, { target: { value: "42" } });
    expect(displayOrderInput.value).toBe("42");
  });

  it("can set parent page to null when selecting empty option", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByRole("option", { name: "Page 1" })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByRole("combobox", { name: /vælg side/i }), { target: { value: "1" } });
    await waitFor(() => expect(screen.getByLabelText("Overordnet side")).toBeInTheDocument());
    const parentSelect = screen.getByLabelText("Overordnet side") as HTMLSelectElement;
    fireEvent.change(parentSelect, { target: { value: "" } });
    expect(parentSelect.value).toBe("");
  });
});
