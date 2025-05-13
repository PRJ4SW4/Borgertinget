import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import DeleteLearningPage from "../../components/AdminPages/DeleteLearningPage";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

const mockPagesSummary = [
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

const mockPageDetail = {
  id: 1,
  title: "Page to Delete Title",
  content: "Page to Delete Content",
  parentPageId: null,
};

describe("DeleteLearningPage", () => {
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
    mockedAxios.delete.mockResolvedValue({ data: {} });
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
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/pages", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
    });

    mockPagesSummary.forEach((page) => {
      expect(screen.getByText(page.title)).toBeInTheDocument();
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

    await waitFor(() => {
      expect(screen.getByText("Page to Delete")).toBeInTheDocument();
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
    expect(screen.getByLabelText("Indhold")).toHaveValue(
      mockPageDetail.content
    );
    expect(screen.getByText("Slet Side")).toBeInTheDocument();
  });

  it("deletes the page and navigates on successful deletion after confirmation", async () => {
    (window.confirm as Mock).mockReturnValue(true);

    render(
      <BrowserRouter>
        <DeleteLearningPage />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(screen.getByText("Page to Delete")).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByText("Slet Side")).toBeInTheDocument();
    });

    const deleteButton = screen.getByText("Slet Side");
    fireEvent.click(deleteButton);

    expect(window.confirm).toHaveBeenCalledWith(
      "Er du sikker på, at du vil slette denne læringsside?"
    );

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
      expect(screen.getByText("Page to Delete")).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByText("Slet Side")).toBeInTheDocument();
    });

    const deleteButton = screen.getByText("Slet Side");
    fireEvent.click(deleteButton);

    expect(window.confirm).toHaveBeenCalledWith(
      "Er du sikker på, at du vil slette denne læringsside?"
    );
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
      expect(screen.getByText("Page to Delete")).toBeInTheDocument();
    });

    const selectPage = screen.getByRole("combobox", { name: /vælg side/i });
    fireEvent.change(selectPage, { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByText("Slet Side")).toBeInTheDocument();
    });

    const deleteButton = screen.getByText("Slet Side");
    fireEvent.click(deleteButton);

    await waitFor(() => {
      expect(mockedAxios.delete).toHaveBeenCalled();
    });

    expect(window.alert).toHaveBeenCalledWith("Sletning fejlede.");
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
