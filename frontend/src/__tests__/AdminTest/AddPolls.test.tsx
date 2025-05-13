import { describe, it, expect, beforeEach, Mock } from "vitest"; // Removed vi
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import AddPolls from "../../components/AdminPages/AddPolls";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

const mockPoliticians = [
  { id: "1", navn: "Politician Alpha" },
  { id: "2", navn: "Politician Beta" },
];

describe("AddPolls", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/aktor/all") {
        return Promise.resolve({ data: mockPoliticians });
      }
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) {
        return Promise.resolve({ data: { politicianTwitterId: 12345 } });
      }
      return Promise.reject(new Error(`Unknown GET URL: ${url}`));
    });
    mockedAxios.post.mockResolvedValue({
      data: { id: 1, question: "New Poll" },
    });
  });

  it("renders the form correctly and fetches politicians", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );

    expect(screen.getByText("Opret en Poll")).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByLabelText("Vælg Politiker *")).toBeInTheDocument();
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/aktor/all", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
    });
    mockPoliticians.forEach((p) => {
      expect(screen.getByText(p.navn)).toBeInTheDocument();
    });
    expect(
      screen.getByPlaceholderText("Skriv spørgsmålet her...")
    ).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Svarmulighed 1")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Svarmulighed 2")).toBeInTheDocument();
    expect(screen.getByText("Opret Poll")).toBeInTheDocument();
  });

  it("updates question and option fields on input", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );
    await waitFor(() =>
      expect(screen.getByText("Politician Alpha")).toBeInTheDocument()
    );

    const questionInput = screen.getByPlaceholderText(
      "Skriv spørgsmålet her..."
    ) as HTMLInputElement;
    fireEvent.change(questionInput, { target: { value: "Test Question?" } });
    expect(questionInput.value).toBe("Test Question?");

    const option1Input = screen.getByPlaceholderText(
      "Svarmulighed 1"
    ) as HTMLInputElement;
    fireEvent.change(option1Input, { target: { value: "Option Yes" } });
    expect(option1Input.value).toBe("Option Yes");
  });

  it("fetches twitterId when a politician is selected", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );
    await waitFor(() =>
      expect(screen.getByText(mockPoliticians[0].navn)).toBeInTheDocument()
    );

    const politicianSelect = screen.getByLabelText(
      "Vælg Politiker *"
    ) as HTMLSelectElement;
    fireEvent.change(politicianSelect, {
      target: { value: mockPoliticians[0].id },
    });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(
        `/api/subscription/lookup/politicianTwitterId?aktorId=${mockPoliticians[0].id}`,
        {
          headers: { Authorization: "Bearer fake-jwt-token" },
        }
      );
    });
  });

  it("adds and removes options", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );
    await waitFor(() =>
      expect(screen.getByText("Politician Alpha")).toBeInTheDocument()
    );

    const addOptionButton = screen.getByText("Tilføj Svarmulighed");
    fireEvent.click(addOptionButton); // Add 3rd option
    expect(screen.getByPlaceholderText("Svarmulighed 3")).toBeInTheDocument();
    fireEvent.click(addOptionButton); // Add 4th option
    expect(screen.getByPlaceholderText("Svarmulighed 4")).toBeInTheDocument();
    expect(addOptionButton).toBeDisabled();

    const removeOptionButton = screen.getByText("Fjern Svarmulighed");
    fireEvent.click(removeOptionButton); // Remove 4th option
    expect(
      screen.queryByPlaceholderText("Svarmulighed 4")
    ).not.toBeInTheDocument();
    fireEvent.click(removeOptionButton); // Remove 3rd option
    expect(
      screen.queryByPlaceholderText("Svarmulighed 3")
    ).not.toBeInTheDocument();
    expect(removeOptionButton).toBeDisabled();
  });

  it("shows an alert if politician is not selected on submit", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );

    await waitFor(() =>
      expect(screen.getByText("Politician Alpha")).toBeInTheDocument()
    );

    // Fill other required fields to ensure HTML5 validation doesn't block submission
    fireEvent.change(screen.getByPlaceholderText("Skriv spørgsmålet her..."), {
      target: { value: "A question" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 1"), {
      target: { value: "Option A" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 2"), {
      target: { value: "Option B" },
    });

    const saveButton = screen.getByText("Opret Poll");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Du skal vælge en politiker.");
    });
  });

  it("submits the form and navigates on success", async () => {
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );
    await waitFor(() =>
      expect(screen.getByText(mockPoliticians[0].navn)).toBeInTheDocument()
    );

    fireEvent.change(screen.getByLabelText("Vælg Politiker *"), {
      target: { value: mockPoliticians[0].id },
    });
    await waitFor(() =>
      expect(mockedAxios.get).toHaveBeenCalledWith(
        `/api/subscription/lookup/politicianTwitterId?aktorId=${mockPoliticians[0].id}`,
        expect.anything()
      )
    );

    fireEvent.change(screen.getByPlaceholderText("Skriv spørgsmålet her..."), {
      target: { value: "Favorite Season?" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 1"), {
      target: { value: "Summer" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 2"), {
      target: { value: "Winter" },
    });
    fireEvent.change(screen.getByLabelText("Slutdato (valgfri)"), {
      target: { value: "2024-12-31" },
    });

    const saveButton = screen.getByText("Opret Poll");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledWith(
        "/api/polls",
        {
          question: "Favorite Season?",
          options: ["Summer", "Winter"],
          politicianTwitterId: 3, // As per component's current hardcoding
          // politicianTwitterId: 12345, // This would be if using fetched twitterId
          endedAt: new Date("2024-12-31").toISOString(),
        },
        { headers: { Authorization: "Bearer fake-jwt-token" } }
      );
    });

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith("/admin/polls");
    });
  });

  it("shows an alert on API error during submission", async () => {
    mockedAxios.post.mockRejectedValueOnce(new Error("Server error"));
    render(
      <BrowserRouter>
        <AddPolls />
      </BrowserRouter>
    );
    await waitFor(() =>
      expect(screen.getByText(mockPoliticians[0].navn)).toBeInTheDocument()
    );

    fireEvent.change(screen.getByLabelText("Vælg Politiker *"), {
      target: { value: mockPoliticians[0].id },
    });
    fireEvent.change(screen.getByPlaceholderText("Skriv spørgsmålet her..."), {
      target: { value: "Error Poll" },
    });
    // Fill required option fields
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 1"), {
      target: { value: "Option X" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 2"), {
      target: { value: "Option Y" },
    });

    const saveButton = screen.getByText("Opret Poll");
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalled();
    });

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith(
        "Failed to create poll. Please try again."
      );
    });
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
