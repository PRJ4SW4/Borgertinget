import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import EditPoll from "../../components/AdminPages/EditPoll";
import { mockNavigate, mockedAxios, mockGetItem } from "../testMocks";

const mockPollsSummaryList = [
  { id: 1, question: "Poll One Question", politicianTwitterId: "twit1" },
  { id: 2, question: "Poll Two Question", politicianTwitterId: "twit2" },
];

const mockPoliticiansList = [
  { id: "aktor1", navn: "Politician Uno", politicianTwitterId: 111 },
  { id: "aktor2", navn: "Politician Dos", politicianTwitterId: 222 },
];

interface MockPollOption {
  id: number;
  optionText: string;
}

interface MockPollDetails {
  id: number;
  question: string;
  options: MockPollOption[];
  endedAt: string;
  politicianId: string;
}

const mockSelectedPollDetails: MockPollDetails = {
  id: 1,
  question: "Poll One Question Detailed",
  options: [
    { id: 101, optionText: "Opt A" },
    { id: 102, optionText: "Opt B" },
  ],
  endedAt: "2024-01-01T00:00:00Z",
  politicianId: "twitterIdForAktor1",
};

describe("EditPoll", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") {
        return Promise.resolve({ data: mockPollsSummaryList });
      }
      if (url === "/api/aktor/all") {
        return Promise.resolve({ data: mockPoliticiansList });
      }
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) {
        return Promise.resolve({ data: mockSelectedPollDetails });
      }
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) {
        const aktorId = url.split("=")[1];
        const politician = mockPoliticiansList.find((p) => p.id === aktorId);
        return Promise.resolve({
          data: { politicianTwitterId: politician?.politicianTwitterId || 789 },
        });
      }
      if (url.startsWith("/api/administrator/lookup/aktorId")) {
        return Promise.resolve({ data: { aktorId: "aktor1" } });
      }
      return Promise.reject(new Error(`Unknown GET URL: ${url}`));
    });
    mockedAxios.put.mockResolvedValue({ data: {} });
  });

  it("renders, fetches polls and politicians", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    expect(screen.getByText("Rediger Poll")).toBeInTheDocument();
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/polls", expect.anything());
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/aktor/all", expect.anything());
    });
    mockPollsSummaryList.forEach((p) => expect(screen.getByText(p.question)).toBeInTheDocument());
    expect(screen.queryByLabelText("Vælg Politiker *")).not.toBeInTheDocument();
  });

  it("fetches and displays poll details when a poll is selected", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    const pollSelect = screen.getByLabelText("Vælg Poll") as HTMLSelectElement;
    fireEvent.change(pollSelect, {
      target: { value: String(mockSelectedPollDetails.id) },
    });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(`/api/polls/${mockSelectedPollDetails.id}`, expect.anything());
      expect(mockedAxios.get).toHaveBeenCalledWith(
        `/api/administrator/lookup/aktorId?twitterId=${mockSelectedPollDetails.politicianId}`,
        expect.anything()
      );
    });

    await waitFor(() => {
      expect(screen.getByLabelText("Vælg Politiker *")).toBeInTheDocument();
      expect(screen.getByDisplayValue(mockPoliticiansList[0].navn)).toBeInTheDocument();
    });

    expect(screen.getByPlaceholderText("Skriv spørgsmålet her...")).toHaveValue(mockSelectedPollDetails.question);
    expect(screen.getByPlaceholderText("Svarmulighed 1")).toHaveValue(mockSelectedPollDetails.options[0].optionText);
    expect(screen.getByLabelText("Slutdato (valgfri)")).toHaveValue("2024-01-01");
  });

  it("submits updated form data", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Skriv spørgsmålet her...")).toHaveValue(mockSelectedPollDetails.question);
    });

    const politicianSelect = screen.getByLabelText("Vælg Politiker *") as HTMLSelectElement;
    fireEvent.change(politicianSelect, {
      target: { value: mockPoliticiansList[1].id },
    });

    // await waitFor(() => {
    //   expect(mockedAxios.get).toHaveBeenCalledWith(
    //     `/api/subscription/lookup/politicianTwitterId?aktorId=${mockPoliticiansList[1].id}`,
    //     expect.anything()
    //   );
    // });

    fireEvent.change(screen.getByPlaceholderText("Skriv spørgsmålet her..."), {
      target: { value: "Updated Question?" },
    });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 1"), {
      target: { value: "Updated Opt A" },
    });

    fireEvent.click(screen.getByText("Opdater Poll"));

    await waitFor(() => {
      expect(mockedAxios.put).toHaveBeenCalledWith(
        `/api/polls/${mockSelectedPollDetails.id}`,
        {
          question: "Updated Question?",
          options: ["Updated Opt A", mockSelectedPollDetails.options[1].optionText],
          politicianTwitterId: undefined,
          endedAt: new Date("2024-01-01T00:00:00Z").toISOString(),
        },
        expect.anything()
      );
    });
    expect(mockNavigate).toHaveBeenCalledWith("/admin/polls");
  });

  it("shows alert if politician is not selected on submit", async () => {
    // Test-specific mock implementation for axios.get
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") {
        return Promise.resolve({ data: mockPollsSummaryList });
      }
      if (url === "/api/aktor/all") {
        return Promise.resolve({ data: mockPoliticiansList });
      }
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) {
        // Crucial for this test: return poll details with politicianId as null
        return Promise.resolve({
          data: { ...mockSelectedPollDetails, politicianId: null },
        });
      }
      // Provide default successful responses for other potential GET calls to avoid unhandled rejections
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) {
        return Promise.resolve({ data: { politicianTwitterId: 12345 } }); // Default mock value
      }
      if (url.startsWith("/api/administrator/lookup/aktorId")) {
        return Promise.resolve({ data: { aktorId: "aktor1" } }); // Default mock value
      }
      return Promise.reject(new Error(`Unhandled GET URL in test-specific mock: ${url}`));
    });

    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );

    // Wait for initial data to load (polls list)
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    // Select the poll that will have null politicianId
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });

    // Wait for the form to populate and verify politician selection is empty
    await waitFor(() => {
      expect(screen.getByPlaceholderText("Skriv spørgsmålet her...")).toBeInTheDocument();
      const politicianSelect = screen.getByLabelText("Vælg Politiker *") as HTMLSelectElement;
      expect(politicianSelect).toBeInTheDocument();
      expect(politicianSelect.value).toBe(""); // Ensure no politician is selected
    });

    // Attempt to submit the form
    fireEvent.click(screen.getByText("Opdater Poll"));

    // Check that the alert was called
    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Du skal vælge en politiker.");
    });
    // Ensure navigation did not occur
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it("alerts if not logged in when fetching polls", async () => {
    mockGetItem.mockReturnValueOnce(null); // No token
    const alertSpy = vi.spyOn(window, "alert");
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(alertSpy).toHaveBeenCalledWith("Du er ikke logget ind.");
    });
    alertSpy.mockRestore();
  });

  it("logs error if fetching polls fails", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    (mockedAxios.get as Mock).mockImplementationOnce(() => Promise.reject(new Error("fail")));
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(errorSpy).toHaveBeenCalledWith("Failed to fetch polls", expect.any(Error));
    });
    errorSpy.mockRestore();
  });

  it("alerts if not logged in when fetching politicians", async () => {
    mockGetItem.mockReturnValueOnce(null); // No token
    const alertSpy = vi.spyOn(window, "alert");
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(alertSpy).toHaveBeenCalledWith("Du er ikke logget ind.");
    });
    alertSpy.mockRestore();
  });

  it("logs error if fetching politicians fails", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") return Promise.resolve({ data: mockPollsSummaryList });
      if (url === "/api/aktor/all") return Promise.reject(new Error("fail"));
      // Provide a default for poll details to avoid unhandled
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) return Promise.resolve({ data: mockSelectedPollDetails });
      return Promise.reject(new Error("unhandled"));
    });
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => {
      expect(errorSpy).toHaveBeenCalledWith("Failed to fetch politicians", expect.any(Error));
    });
    errorSpy.mockRestore();
  });

  it("does not fetch twitterId if no politician is selected", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    // Should not call axios.get for twitterId
    expect(mockedAxios.get).not.toHaveBeenCalledWith(expect.stringContaining("/api/subscription/lookup/politicianTwitterId"), expect.anything());
  });

  it("sets twitterId to null and logs error if fetchTwitterId fails", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") return Promise.resolve({ data: mockPollsSummaryList });
      if (url === "/api/aktor/all") return Promise.resolve({ data: mockPoliticiansList });
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) return Promise.resolve({ data: mockSelectedPollDetails });
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) return Promise.reject(new Error("twitterId fail"));
      if (url.startsWith("/api/administrator/lookup/aktorId")) return Promise.resolve({ data: { aktorId: "aktor1" } });
      return Promise.reject(new Error("unhandled"));
    });
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });
    await waitFor(() => expect(screen.getByLabelText("Vælg Politiker *")).toBeInTheDocument());
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    fireEvent.change(screen.getByLabelText("Vælg Politiker *"), {
      target: { value: mockPoliticiansList[0].id },
    });
    await waitFor(() => {
      expect(errorSpy).toHaveBeenCalledWith("Could not fetch politicianTwitterId", expect.any(Error));
    });
    errorSpy.mockRestore();
  });

  it("logs error if fetchPollDetails fails", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") return Promise.resolve({ data: mockPollsSummaryList });
      if (url === "/api/aktor/all") return Promise.resolve({ data: mockPoliticiansList });
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) return Promise.reject(new Error("fetchPollDetails fail"));
      return Promise.reject(new Error("unhandled"));
    });
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });
    await waitFor(() => {
      expect(errorSpy).toHaveBeenCalledWith("Failed to fetch poll details", expect.any(Error));
    });
    errorSpy.mockRestore();
  });

  it("logs error and sets selectedPoliticianId to null if aktorId lookup fails in fetchPollDetails", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") return Promise.resolve({ data: mockPollsSummaryList });
      if (url === "/api/aktor/all") return Promise.resolve({ data: mockPoliticiansList });
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) return Promise.resolve({ data: mockSelectedPollDetails });
      if (url.startsWith("/api/administrator/lookup/aktorId")) return Promise.reject(new Error("aktorId fail"));
      return Promise.reject(new Error("unhandled"));
    });
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });
    await waitFor(() => {
      expect(errorSpy).toHaveBeenCalledWith("Could not resolve aktorId from twitterId", expect.any(Error));
    });
    errorSpy.mockRestore();
  });

  it("can add and remove answer options", async () => {
    // Ensure all mocks are set up for this test
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") return Promise.resolve({ data: mockPollsSummaryList });
      if (url === "/api/aktor/all") return Promise.resolve({ data: mockPoliticiansList });
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) return Promise.resolve({ data: mockSelectedPollDetails });
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) return Promise.resolve({ data: { politicianTwitterId: 12345 } });
      if (url.startsWith("/api/administrator/lookup/aktorId")) return Promise.resolve({ data: { aktorId: "aktor1" } });
      return Promise.reject(new Error("unhandled"));
    });
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });
    await waitFor(() => expect(screen.getByPlaceholderText("Svarmulighed 1")).toBeInTheDocument());
    const addOptionBtn = screen.getByText("Tilføj Svarmulighed");
    fireEvent.click(addOptionBtn);
    expect(screen.getAllByPlaceholderText(/Svarmulighed/).length).toBe(3);
    fireEvent.click(addOptionBtn);
    expect(screen.getAllByPlaceholderText(/Svarmulighed/).length).toBe(4);
    expect(addOptionBtn).toBeDisabled();
    const removeOptionBtn = screen.getByText("Fjern Svarmulighed");
    fireEvent.click(removeOptionBtn);
    expect(screen.getAllByPlaceholderText(/Svarmulighed/).length).toBe(3);
    fireEvent.click(removeOptionBtn);
    expect(screen.getAllByPlaceholderText(/Svarmulighed/).length).toBe(2);
    expect(removeOptionBtn).toBeDisabled();
  });

  it("can change the end date", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), {
      target: { value: String(mockSelectedPollDetails.id) },
    });
    await waitFor(() => expect(screen.getByLabelText("Slutdato (valgfri)")).toBeInTheDocument());
    const endDateInput = screen.getByLabelText("Slutdato (valgfri)") as HTMLInputElement;
    fireEvent.change(endDateInput, { target: { value: "2025-05-25" } });
    expect(endDateInput.value).toBe("2025-05-25");
  });
});
