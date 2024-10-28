package com.drunkengames.hangover.domain.rank.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import java.util.List;

@AllArgsConstructor
@NoArgsConstructor
@Getter
@Setter
public class RankResponseDto {
    private List<RankDto> rankList;
    private RankDto myRank;
}
