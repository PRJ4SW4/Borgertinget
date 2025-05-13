import {
  render,
  screen,
  fireEvent,
  waitFor,
  within,
} from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { BrowserRouter } from "react-router-dom";
import EditLearningPage from "../../components/AdminPages/EditLearningPage";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

const mockPagesSummary = [
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

const mockPageDetail = {
  id: 1,
  title: "Page 1 Title",
  content: "Page 1 Content",
  parentPageId: null,
};

describe("EditLearningPage", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    mockedAxios.get.mockImplementation((url) => {
      if (url === "/api/pages") {
        return Promise.resolve({ data: mockPagesSummary });
      }
      if (url === "/api/pages/1") {
        return Promise.resolve({ data: mockPageDetail });
      }
      return Promise.reject(new Error("not found"));
    });
    mockedAxios.put.mockResolvedValue({ data: {} });
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
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/pages", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
    });

    mockPagesSummary.forEach((page) => {
      expect(screen.getByText(page.title)).toBeInTheDocument();
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
      expect(screen.getByText("Page 1")).toBeInTheDocument();
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

    expect(screen.getByPlaceholderText("Titel")).toHaveValue(
      mockPageDetail.title
    );
    expect(screen.getByPlaceholderText("Markdown indhold")).toHaveValue(
      mockPageDetail.content
    );
    expect(screen.getByText("Gem Ændringer")).toBeInTheDocument();
  });

  it("updates form fields on input", async () => {
    render(
      <BrowserRouter>
        <EditLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByText("Page 1")).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Titel")).toBeInTheDocument();
    });

    const titleInput = screen.getByPlaceholderText("Titel") as HTMLInputElement;
    fireEvent.change(titleInput, { target: { value: "Updated Title" } });
    expect(titleInput.value).toBe("Updated Title");

    const contentTextarea = screen.getByPlaceholderText(
      "Markdown indhold"
    ) as HTMLTextAreaElement;
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
      expect(screen.getByText("Page 1")).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Titel")).toHaveValue(
        mockPageDetail.title
      );
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
      expect(screen.getByText("Page 1")).toBeInTheDocument();
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
});
