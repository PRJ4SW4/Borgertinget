import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import DeletePoll from "../components/AdminPages/DeletePoll";
import { mockNavigate, mockedAxios, mockGetItem } from "./testMocks";

const mockPollsSummaryList = [
  { id: 1, question: "Poll To Delete Question" },
  { id: 2, question: "Another Poll Question" },
];

const mockPoliticiansList = [
  { id: "aktor1", navn: "Politician Uno" },
  { id: "aktor2", navn: "Politician Dos" },
];

const mockSelectedPollDetails: any = {
  id: 1,
  question: "Poll To Delete Question Detailed",
  options: [
    { id: 101, optionText: "Delete Opt A" },
    { id: 102, optionText: "Delete Opt B" },
  ],
  endedAt: "2024-02-01T00:00:00Z",
  politicianId: "aktor1",
};

describe("DeletePoll", () => {
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
      if (url === `/api/administrator/lookup/aktorId?twitterId=${mockSelectedPollDetails.politicianId}`) {
        return Promise.resolve({ data: { aktorId: mockSelectedPollDetails.politicianId } });
      }
      return Promise.reject(new Error(`Unknown GET URL: ${url}`));
    });
    mockedAxios.delete.mockResolvedValue({ data: {} });
  });

  it("renders, fetches polls and politicians", async () => {
    render(
      <BrowserRouter>
        <DeletePoll />
      </BrowserRouter>
    );
    expect(screen.getByText("Slet Poll")).toBeInTheDocument();
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/polls", expect.anything());
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/aktor/all", expect.anything());
    });
    mockPollsSummaryList.forEach((p) => expect(screen.getByText(p.question)).toBeInTheDocument());
    expect(screen.queryByText("Slet Poll", { selector: "button" })).not.toBeInTheDocument();
  });

  it("fetches and displays poll details (disabled) when a poll is selected", async () => {
    render(
      <BrowserRouter>
        <DeletePoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    const pollSelect = screen.getByLabelText("Vælg Poll") as HTMLSelectElement;
    fireEvent.change(pollSelect, { target: { value: String(mockSelectedPollDetails.id) } });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(`/api/polls/${mockSelectedPollDetails.id}`, expect.anything());
    });

    await waitFor(() => {
      const questionInput = screen.getByPlaceholderText("Skriv spørgsmålet her...") as HTMLInputElement;
      expect(questionInput).toHaveValue(mockSelectedPollDetails.question);
      expect(questionInput).toBeDisabled();

      const optionInput = screen.getByPlaceholderText("Svarmulighed 1") as HTMLInputElement;
      expect(optionInput).toHaveValue(mockSelectedPollDetails.options[0].optionText);
      expect(optionInput).toBeDisabled();

      const politicianSelect = screen.getByLabelText("Vælg Politiker") as HTMLSelectElement;
      expect(politicianSelect).toHaveValue(mockSelectedPollDetails.politicianId);
      expect(politicianSelect).toBeDisabled();

      expect(screen.getByText("Slet Poll", { selector: "button" })).toBeInTheDocument();
    });
  });

  it("deletes the poll and navigates on confirmation", async () => {
    (window.confirm as Mock).mockReturnValue(true);
    render(
      <BrowserRouter>
        <DeletePoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), { target: { value: String(mockSelectedPollDetails.id) } });

    await waitFor(() => expect(screen.getByText("Slet Poll", { selector: "button" })).toBeInTheDocument());
    fireEvent.click(screen.getByText("Slet Poll", { selector: "button" }));

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne poll?");
    await waitFor(() => {
      expect(mockedAxios.delete).toHaveBeenCalledWith(`/api/polls/${mockSelectedPollDetails.id}`, expect.anything());
    });
    expect(mockNavigate).toHaveBeenCalledWith("/admin/polls");
  });

  it("does not delete if confirmation is cancelled", async () => {
    (window.confirm as Mock).mockReturnValue(false);
    render(
      <BrowserRouter>
        <DeletePoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText("Vælg Poll"), { target: { value: String(mockSelectedPollDetails.id) } });

    await waitFor(() => expect(screen.getByText("Slet Poll", { selector: "button" })).toBeInTheDocument());
    fireEvent.click(screen.getByText("Slet Poll", { selector: "button" }));

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne poll?");
    expect(mockedAxios.delete).not.toHaveBeenCalled();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
